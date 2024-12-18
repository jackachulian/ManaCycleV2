using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A board that can be controlled by a player or an AI.
/// Manages the tile grid and movement/placement of pieces.
/// </summary>
public class Board : MonoBehaviour
{
    /// <summary>
    /// All ManaTiles that have been placed on the board
    /// (This does not include any currently falling pieces on this board!)
    /// </summary>
    [SerializeField] private ManaTile[,] manaTileGrid;
    [SerializeField] private int width = 7, height = 15;

    /// <summary>
    /// How often the current piece should fall down one tile
    /// </summary>
    [SerializeField] private float movementFrequency = 0.8f;

    /// <summary>
    /// The battleManager that is managing this board. Also contains dependencies needed for SpawnPiece, etc
    /// Is set upon battle initialization.
    /// </summary>
    private BattleManager battleManager;

    /// <summary>
    /// Incrementing timer. When greater than movementFrequency (after fall speed modifiers), 
    /// set to 0 and move the current piece down one tile.
    /// </summary>
    private float movementTimer;

    /// <summary>
    /// RNG instance used to determine the colors.
    /// Seed is set on battle initialization.
    /// </summary>
    private System.Random rng;

    /// <summary>
    /// Current falling piece on this board.
    /// </summary>
    private ManaPiece currentPiece;

    /// <summary>
    /// Called by BattleManager when battle is initialized
    /// Any initialization should go here (no Start() method)
    /// </summary>
    /// <param name="battleManager">the battle manager for the battle this board is being initialized within</param>
    /// <param name="seed">the seed to use for RNG</param>
    public void InitializeBattle(BattleManager battleManager, int seed) {
        this.battleManager = battleManager;
        manaTileGrid = new ManaTile[width, height];

        rng = new System.Random(seed);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: receive user input from input manager

        movementTimer += Time.deltaTime;
        if (movementTimer > movementFrequency) {
            movementTimer -= movementFrequency;
            MovePiece(Vector2Int.down);
        }
    }

    void SpawnNewPiece() {
        currentPiece = battleManager.SpawnPiece();

        for (int i = 0; i < currentPiece.tiles.Length; i++) {
            int color = rng.Next(5);
            currentPiece.tiles[i].SetColor(color);
        }
    }

    /// <summary>
    /// Move the current piece. If the movement would cause the piece to be in an invalid placement, return false and undo the transformation.
    /// Otherwise, returns true and updates the visual position of the tiles in the piece.
    /// </summary>
    /// <param name="offset">the offset of the piece, in board grid space</param>
    /// <returns>true if the tile was able to move.</returns>
    bool MovePiece(Vector2Int offset) {
        // TODO: implement
        return true;
    }

    /// <summary>
    /// Returns true if all tiles in the current piece are within bounds and not obstructed by any tiles already placed.
    /// </summary>
    bool CheckValidPlacement() {
        // TODO: check each tile for valid position
        return true;
    }
}
