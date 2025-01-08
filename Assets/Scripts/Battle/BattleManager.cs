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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate GameManager spawned, destroying the newly instantiated one");
            Destroy(gameObject);
        }
    }

    private void Start() {
        // TESTING ONLY - create new battle data and use that in the game manager, then initialize the battle
        BattleData battleData = new BattleData();
        battleData.cycleUniqueColors = 5;
        battleData.cycleLength = 7;
        battleData.Randomize();
        GameManager.Instance.battleData = battleData;

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

        // Initialize the cycle and generate a random sequence of colors.
        // The board RNG is not used for this.
        manaCycle.InitializeBattle(this);
        
        foreach (Board board in boards) {
            board.InitializeBattle(this, GameManager.Instance.battleData.seed);
            board.onDefeat.AddListener(CheckForWinner);
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
        return boards[index];
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
        postGameMenuUI.ShowPostGameMenuUI();
    }

    /// <summary>
    /// Go back to the character select screen.
    /// Only works on server/host.
    /// </summary>
    public void GoToCharacterSelect() {
        
    }
}