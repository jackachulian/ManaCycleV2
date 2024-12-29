using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
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

    private void Awake() {
        if (battleLobbyManager.battleManager != null) {
            Debug.LogWarning("Duplicate BattleManager! Destroying the old one.");
            Destroy(battleLobbyManager.battleManager.gameObject);
        }
        
        battleLobbyManager.battleManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE;
    }

    public void Start() {
        battleLobbyManager.StartNetworkManagerHost();

        // Initialize the cycle and generate a random sequence of colors.
        // The board RNG is not used for this.
        manaCycle.InitializeBattle(this);

        // Used for mana color RNG during the match.
        // All boards share the same seed, and will have the same piece colors if the same RNG mode is selected.

        foreach (Board board in boards) {
            board.InitializeBattle(this, battleLobbyManager.battleData.seed);
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
                board.GetComponent<NetworkObject>().SpawnWithOwnership(player.OwnerClientId);
            }
        } else {
            Debug.Log("This is not the server");
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