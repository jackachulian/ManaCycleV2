using UnityEngine;

public class BattleManager : MonoBehaviour
{
    /// <summary>
    /// The BattleManager for the scene. Only one should exist at a time, a warning will be raised if there is more than one.
    /// </summary>
    public static BattleManager instance {get; set;}

    /// <summary>
    /// The current settings being used for the battle, such as RNG seed, etc
    /// </summary>
    public static BattleSettings battleSettings {get; private set;}

    /// <summary>
    /// The Mana Cycle object that dictates the order of color clears.
    /// </summary>
    [SerializeField] public ManaCycle manaCycle;

    /// <summary>
    /// All players in the game. 2-4 players per game
    /// </summary>
    [SerializeField] private Board[] boards;

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
    /// Use this to set all the settings to use for the battle.
    /// </summary>
    /// <param name="settings">the BattleSettings object to use</param>
    public static void Configure(BattleSettings settings) {
        battleSettings = settings;
    }

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("A new BattleManager has replaced the old one! Make sure there is only one BattleManager.");
        }

        instance = this;
    }

    public void Start() {
        // TODO: synchronize RNG between clients

        // Initialize the cycle and generate a random sequence of colors.
        // The board RNG is not used for this.
        manaCycle.InitializeBattle(this);

        // Used for mana color RNG during the match.
        // All boards share the same seed, and will have the same piece colors if the same RNG mode is selected.

        foreach (Board board in boards) {
            board.InitializeBattle(this, battleSettings.seed);
        }

        // Connects all players found to their respective boards.
        // This works for both online and local, as both use BattlePlayers.
        var players = FindObjectsByType<BattlePlayer>(FindObjectsSortMode.None);
        Debug.Log("Players: "+players+" - count: "+players.Length);

        foreach (BattlePlayer player in players) {
            player.BattleConnectBoard();
            player.EnableBattleInputs();
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
}