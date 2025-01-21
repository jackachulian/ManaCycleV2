using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the current battle in the battle scene this is in.
/// </summary>
public class BattleManager : MonoBehaviour
{   
    public static BattleManager Instance { get; private set; }

    /// <summary>
    /// Manages what board layout is being used based on the amount of players.
    /// Holds a reference to the current layout which contains all the boards.
    /// </summary>
    public BoardLayoutManager boardLayoutManager;

    /// <summary>
    /// Accessor to quickly retrieve the mana cycle object on the current layout
    /// </summary>
    public ManaCycle manaCycle => boardLayoutManager.currentLayout.manaCycle;

    /// <summary>
    /// UI for the postgame. Start the UI when the game completes.
    /// The postgame UI will send requests to this BattleManager if a rematch / character select is requested
    /// </summary>
    [SerializeField] private PostGameMenuUI postGameMenuUI;

    /// <summary>
    /// Cosmetics to use for all boards and the mana cycle
    /// </summary>
    [SerializeField] public ManaCosmetics cosmetics;

    /// <summary>
    /// Collection of mana visuals created during runtime in BattleManager's InitializeBattle. Used for showing what mana is connected to ghost tiles and will be cleared if placed in that position.
    /// </summary>
    public ManaVisual[] pulseGlowManaVisuals { get; private set; }

    /// <summary>
    /// The L-shaped Triomino ManaPiece that will be duplicated, spawned, and color-changed on all boards
    /// </summary>
    [SerializeField] private ManaPiece manaPiecePrefab;

    /// <summary>
    /// Single seperated mana piece prefab, used for the cycle, abilities, etc
    /// </summary>
    [SerializeField] private ManaTile manaTilePrefab;

    /// <summary>
    /// Used for restarting matches / going back to character selector
    /// </summary>
    [SerializeField] private GameStartNetworkBehaviour _gameStartNetworkBehaviour;
    public GameStartNetworkBehaviour gameStartNetworkBehaviour => _gameStartNetworkBehaviour;

    /// <summary>
    /// only true once initialized. will initialize the first time the networkvariable BattleData is changed/set on this client
    /// </summary>
    public bool initialized {get; private set;} = false;

    /// <summary>
    /// Is set to true when a winner is chosen.
    /// </summary>
    public bool gameCompleted {get; private set;}

    /// <summary>
    /// The Mana Cycle object that dictates the order of color clears. cached on awake
    /// </summary>
    private BattleCountdown countdown;


    /// <summary>
    /// Event invoked when the battle is initialized
    /// </summary>
    public delegate void BattleInitializedCallback();
    public event BattleInitializedCallback BattleInitializedNotifier;

    void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate GameManager spawned, destroying the newly instantiated one");
            Destroy(gameObject);
        }

        _gameStartNetworkBehaviour = GetComponent<GameStartNetworkBehaviour>();
        countdown = GetComponent<BattleCountdown>();
    }

    private void Start() {
        // When battle scene starts, if there is no game active, scene was likely loaded straight into from editor, 
        // start a game in Local Multiplayer mode
        if (!GameManager.Instance) {
            Debug.LogError("A GameManager is required for the battle scene to work!");
            return;
        }

        // Default to starting a game in singleplayer if a game is not active
        if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.None) {
            GameManager.Instance.StartGameHost(GameManager.GameConnectionType.Singleplayer);
            GameManager.Instance.SetGameState(GameManager.GameState.Countdown);
            BattleData battleData = new BattleData();
            battleData.SetDefaults();
            battleData.Randomize();
            GameManager.Instance.SetBattleData(battleData);
            Debug.Log("Battle data generated within battle scene - seed: "+battleData.seed);
        }

        InitializeBattle();

        // TODO: set to Countdown, and then set to Playing after the synchronized countdown finishes
        GameManager.Instance.SetGameState(GameManager.GameState.Countdown);
        GameManager.Instance.playerManager.AttachPlayersToBoards();
        GameManager.Instance.playerManager.EnableBattleInputs();

        // If the countdown isn't running, start it (only if this is the server!)
        // BattleStartNetworkManager should handle this, but not if battle scene is loaded straight into
        if (GameManager.Instance.currentConnectionType != GameManager.GameConnectionType.OnlineMultiplayer && NetworkManager.Singleton.IsServer) {
            countdown.StartCountdownServer();
        }
    }

    public void GenerateGlowMaterials()
    {
        // Generate the pulse-glow materials
        pulseGlowManaVisuals = new ManaVisual[cosmetics.manaVisuals.Length];
        // fadeGlowMaterials = new Material[cosmetics.manaVisuals.Length]; <-- moved to board.cs
        for (int i = 0; i < cosmetics.manaVisuals.Length; i++)
        {
            ManaVisual visual = cosmetics.manaVisuals[i];
            ManaVisual pulseGlowVisual = new ManaVisual();

            // TODO: probably should move this to somewhere in the battle cosmetics class, but too lazy rn
            pulseGlowVisual.material = new Material(visual.material);
            pulseGlowVisual.material.SetFloat("_LitAmount", 0.25f);
            pulseGlowVisual.material.SetFloat("_PulseGlowAmplitude", 0.2f);
            pulseGlowVisual.material.SetFloat("_PulseGlowFrequency", 2f);

            pulseGlowVisual.ghostMaterial = new Material(visual.ghostMaterial);
            pulseGlowVisual.ghostMaterial.SetFloat("_LitAmount", 0.25f);
            pulseGlowVisual.ghostMaterial.SetFloat("_PulseGlowAmplitude", 0.2f);
            pulseGlowVisual.ghostMaterial.SetFloat("_PulseGlowFrequency", 2f);

            // fadeGlowMaterials[i] = new Material(visual.material); <-- moved to board.cs

            pulseGlowManaVisuals[i] = pulseGlowVisual;
        }
    }

    /// <summary>
    /// Initialize the battle with the given data. Will decide RNG, cycle sequence, etc
    /// </summary>
    public void InitializeBattle() {
        if (initialized) {
            Debug.LogWarning("BattleManager already initialized");
            return;
        }

        // Choose the layout to use based on the amount of connected players in the gamemanager
        boardLayoutManager.DecideLayout();

        // Initialize the cycle and generate a random sequence of colors.
        // The board RNG is not used for this.
        if (boardLayoutManager.currentLayout.manaCycle) boardLayoutManager.currentLayout.manaCycle.InitializeBattle(this);

        // Create per-battle materials needed for ghost connected tile glowing
        GenerateGlowMaterials();
        
        foreach (Board board in boardLayoutManager.currentLayout.boards) {
            board.InitializeBattle(this, GameManager.Instance.battleData.seed);
            board.onDefeat.AddListener(CheckForWinner);
        }

        foreach (Player player in GameManager.Instance.playerManager.players) {
            if (player.IsOwner) {
                // Set each player as unready so that the next char select will work properly after the upcoming battle
                // (only the player's local client sets these values)
                player.selectedBattlerIndex.Value = -1;
                player.characterChosen.Value = false;
                player.optionsChosen.Value = false;
            }

            // received battle data should also be null for when the next gamestart happens, data isnt leftover from previous game starts
            player.receivedBattleData = default;
        }

        initialized = true;
        BattleInitializedNotifier?.Invoke();
    }

    public static void InstanceStartCountdownServer(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= InstanceStartCountdownServer;
        Instance.countdown.StartCountdownServer();
    }

    public void StartBattle() {
        Debug.Log("Game started!");

        GameManager.Instance.SetGameState(GameManager.GameState.Playing);

        foreach (Board board in boardLayoutManager.currentLayout.boards) {
            board.StartBattle();
        }
    }

    /// <summary>
    /// Allocates a new piece.
    /// Does not set any of the mana colors.
    /// </summary>
    /// <returns>a newly instantiated piece</returns>
    public ManaPiece SpawnPiece() {
        ManaPiece piece = Instantiate(manaPiecePrefab);
        return piece;
    }

    /// <summary>
    /// Allocate one single mana tile and return it.
    /// </summary>
    /// <returns>a newly instantiated tile</returns>
    public ManaTile SpawnTile() {
        ManaTile tile = Instantiate(manaTilePrefab);
        return tile;
    }

    /// <summary>
    /// Get a board by index. Used for assigning boards to player inputs, particularly over the network in online mode.
    /// </summary>
    /// <returns>the board at the given index in teh baords array</returns>
    public Board GetBoardByIndex(int index) {
        return boardLayoutManager.currentLayout.boards[index];
    }

    /// <summary>
    /// Check if only one board remains active and is not defeated (still has health).
    /// </summary>
    public void CheckForWinner() {
        Debug.Log("Checking for winner");

        int livingBoards = 0;
        // tentative winner; will only actually be the winner if no other non-defeated boards are found
        Board winner = null;
        foreach (Board board in boardLayoutManager.currentLayout.boards) {
            if (!board.defeated) {
                livingBoards += 1;
                if (livingBoards == 1) {
                    winner = board;
                } else {
                    return;
                }
            }
        }

        // If no boards are alive, start the postgame, the one player was defeated
        if (livingBoards == 0) {
            gameCompleted = true;
            ServerStartPostGameAfterDelay(winner);
        }

        if (winner) {
            winner.Win();
            gameCompleted = true;
            ServerStartPostGameAfterDelay(winner);
        }
    }

    // Waits a bit and then show the postgame menu
    public async void ServerStartPostGameAfterDelay(Board winner) {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.Log("This isn't the server - waiting for server to start the postgame menu");
            return;
        }

        Debug.Log("Starting the postgame menu after delay as server");

        // TODO: wait until current spellcast completes on winning board
        await Task.Delay(1000);
        gameStartNetworkBehaviour.PostgameMenuRpc(winner.boardIndex);
    }

    public void ClientStartPostGame(Board winner) {
        GameManager.Instance.SetGameState(GameManager.GameState.PostGame);
        postGameMenuUI.ShowPostGameMenuUI(winner);
    }
}