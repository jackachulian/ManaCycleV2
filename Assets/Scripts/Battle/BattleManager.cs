using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the current battle.
/// </summary>
public class BattleManager : NetworkBehaviour
{
    private NetworkVariable<BattleData> battleData = new NetworkVariable<BattleData>();

    /// <summary>
    /// Stores shared battle lobby dependencies
    /// </summary>
    [SerializeField] public BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// The Mana Cycle object that dictates the order of color clears.
    /// </summary>
    [SerializeField] public ManaCycle manaCycle;

    /// <summary>
    /// All players in the game. 2-4 players per game
    /// </summary>
    [SerializeField] private Board[] boards;

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
    /// The L-shaped Triomino ManaPiece that will be duplicated, spawned, and color-changed on all boards
    /// </summary>
    [SerializeField] private ManaPiece manaPiecePrefab;

    /// <summary>
    /// Single seperated mana piece prefab, used for the cycle, abilities, etc
    /// </summary>
    [SerializeField] private ManaTile manaTilePrefab;

    /// <summary>
    /// only true once initialized. will initialize the first time the networkvariable BattleData is changed/set on this client
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// Is set to true when a winner is chosen.
    /// </summary>
    public bool gameCompleted {get; private set;}

    /// <summary>
    /// Used to start rematches
    /// </summary>
    [SerializeField] private BattleStartNetworkBehaviour battleStart;

    private void Awake() {
        if (this == null) {
            Debug.LogWarning("Self battlemanager is null, destroying self");
            Destroy(gameObject);
            return;
        }

        if (!gameObject.activeSelf) {
            Debug.LogWarning("battlemanager is not activeSelf, destroying self");
            Destroy(gameObject);
            return;
        }

        if (battleLobbyManager.battleManager == this) {
            Debug.LogWarning("BattleManager instance awoke twice?");
            return;
        }

        if (battleLobbyManager.battleManager != null) {
            Debug.LogWarning("Duplicate BattleManager! Destroying the old one.");
            Destroy(battleLobbyManager.battleManager.gameObject);
        }

        battleLobbyManager.battleManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE;
    }

    public override void OnNetworkSpawn() {
        Debug.Log("BattleManager spawned");
        if (battleLobbyManager.networkManager.IsServer) {
            battleData.Value = battleLobbyManager.battleData;
        }

        InitializeBattle();
    }

    /// <summary>
    /// Initialize the battle with the given data. Will decide RNG, cycle sequence, etc
    /// </summary>
    public void InitializeBattle() {
        if (initialized) {
            Debug.LogWarning("BattleManager already initialized");
            return;
        }
        initialized = true;

        Debug.Log("BattleManager initialized via RPC");
        battleLobbyManager.SetBattleData(battleData.Value);

        battleLobbyManager.battleManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE;

        // Initialize the cycle and generate a random sequence of colors.
        // The board RNG is not used for this.
        manaCycle.InitializeBattle(this);

        
        foreach (Board board in boards) {
            board.InitializeBattle(this, battleLobbyManager.battleData.seed);
            board.onDefeat.AddListener(CheckForWinner);
        }

        // Server spawns board and assigns ownership based on player's boardIndex
        if (battleLobbyManager.networkManager.IsServer) {
            Debug.Log("This is the server, spawning boards");
            foreach (BattlePlayer player in battleLobbyManager.playerManager.GetPlayers()) {
                if (!player) {
                    Debug.LogError("Null player detected");
                    continue;
                }

                if (player.boardIndex.Value < 0 || player.boardIndex.Value >= boards.Length) {
                    Debug.LogError("Player with ID "+player.GetId()+" has out-of-range board index: "+player.boardIndex.Value);
                    continue;
                };

                Board board = boards[player.boardIndex.Value];
                if (!board) {
                    Debug.LogError("Null board detected at index "+player.boardIndex.Value);
                    continue;
                }

                Debug.Log("Spawning board "+board+" with owner of clientID "+player.OwnerClientId);
                board.GetComponent<NetworkObject>().ChangeOwnership(player.OwnerClientId);
            }
        } else {
            Debug.Log("This is not the server");
        }

        battleLobbyManager.playerManager.ConnectAllPlayersToBoards();
        battleLobbyManager.playerManager.EnableBattleInputs();
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
        return boards[index];
    }

    /// <summary>
    /// To be called when a player connects to one of this battle manager's boards
    /// </summary>
    public void OnPlayerConnectedToBoard(BattlePlayer battlePlayer) {        
        if (battleLobbyManager.networkManager.IsServer) {
            // listen for when their readiness changes to know when to check if all players are ready for a rematch
            battlePlayer.ready.OnValueChanged += battleStart.OnAnyPlayerReadyChanged;
        }
    }

    /// <summary>
    /// Check if only one board remains active and is not defeated (still has health).
    /// </summary>
    public void CheckForWinner() {
        Debug.Log("Checking for winner");

        int livingBoards = 0;
        // tentative winner; will only actually be the winner if no other non-defeated boards are found
        Board winner = null;
        foreach (Board board in boards) {
            if (!board.defeated) {
                livingBoards += 1;
                if (livingBoards == 1) {
                    winner = board;
                } else {
                    return;
                }
            }
        }

        if (winner) {
            winner.Win();
            gameCompleted = true;
            PostGame();
        }
    }

    // Waits a bit and then show the postgame menu
    public async void PostGame() {
        // TODO: wait until current spellcast completes on winning board
        await Task.Delay(1000);

        battleStart.canStartBattle = true;

        // disconnect players from boards, they no longer need to be connected
        foreach (var player in battleLobbyManager.playerManager.GetPlayers()) {
            player.DisconnectFromBattleBoard();
        }

        postGameMenuUI.ShowPostGameMenuUI();
    }

    /// <summary>
    /// Go back to the character select screen.
    /// Only works on server/host.
    /// </summary>
    public void GoToCharacterSelect() {
        if (battleLobbyManager.networkManager.IsServer) {
            battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE_SETUP;
            battleLobbyManager.networkManager.SceneManager.LoadScene("BattleSetup", LoadSceneMode.Single);
        } else {
            Debug.LogWarning("Only the server/host can send session back to character select");
        }
    }
}