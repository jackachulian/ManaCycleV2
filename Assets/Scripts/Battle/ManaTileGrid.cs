using UnityEngine;

/// <summary>
/// A grid of placed tiles on a board.
/// (This grid does not include any currently falling pieces on the board!)
/// </summary>
public class ManaTileGrid : MonoBehaviour {
    /// <summary>
    /// Transform that mana tiles should be parented under
    /// </summary>
    [SerializeField] private Transform _manaTileTransform;
    public Transform manaTileTransform => _manaTileTransform;
    
    /// <summary>
    /// Dimensions of the board's tile grid. 
    /// This is slightly taller than the visual size, due to the fact that pieces can be moved above the top edge of the board.
    /// </summary>
    public readonly int width = 7, height = 20;

    /// <summary>
    /// The visual height of the board. Does not include the extra rows above the visible board where tiles can still exist.
    /// </summary>
    public readonly int visual_height = 15;
    
    /// <summary>
    /// Two-dimensional grid storing all tiles.
    /// </summary>
    private ManaTile[,] grid;

    /// <summary>
    /// Called after this grid's Board is initialized.
    /// </summary>
    public void InitializeBattle() {
        grid = new ManaTile[width, height];
    }

    /// <summary>
    /// Returns true if there is a tile at the given x and y coordinate.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns>true if there is a tile here.</returns>
    public bool HasTile(Vector2Int position) {
        return grid[position.x, position.y] != null;
    }

    /// <summary>
    /// Place the tile at the given position.
    /// </summary>
    /// <param name="tile">the tile to place</param>
    /// <param name="position">the grid coords to place the tile at</param>
    public void PlaceTile(ManaTile tile, Vector2Int position) {
        grid[position.x, position.y] = tile;
    }

    /// <summary>
    /// Perform gravity on the tile at the given position.
    /// </summary>
    /// <param name="position">the position of the tile on the grid to fall</param>
    public void TileGravity(Vector2Int position) {
        // if null, no tile here
        ManaTile tile = grid[position.x, position.y];
        if (tile == null) return;

        // While the tile is above the bottom of the board, keep falling
        while (position.y > 0) {
            // If the tile below is not empty, tile cannot fall here
            if (grid[position.x, position.y - 1] != null) {
                break;
            }

            // Fall by one tile
            grid[position.x, position.y] = null;
            position.y -= 1;
            grid[position.x, position.y] = tile;
        }

        // if tile fell at all, animate its fall position to the new position
        if (position.y != tile.position.y) {
            tile.AnimatePosition(position);
        }
    }
}