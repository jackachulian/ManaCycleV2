using UnityEngine;

public class BattleManager : MonoBehaviour
{
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
    [SerializeField] private ManaPiece manaPiece;

    public void Start() {
        // Used for mana color RNG during the match.
        // All boards share the same seed, and will have the same piece colors if the same RNG mode is selected.
        int seed = Random.Range(0, int.MaxValue);

        foreach (Board board in boards) {
            board.InitializeBattle(this, seed);
        }
    }

    /// <summary>
    /// Allocates a new piece.
    /// Does not set any of the mana colors.
    /// Is requested by Board, which then sets the colors based on the Board's RNG state.
    /// </summary>
    public ManaPiece SpawnPiece() {
        ManaPiece piece = Instantiate(manaPiece);
        return piece;
    }
}
