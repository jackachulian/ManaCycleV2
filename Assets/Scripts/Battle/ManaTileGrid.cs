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

    public bool IsInBounds(Vector2Int position) {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    /// <summary>
    /// Returns true if there is a tile at the given x and y coordinate.
    /// </summary>
    public bool HasTile(Vector2Int position) {
        return grid[position.x, position.y] != null;
    }

    /// <summary>
    /// Get the tile at the given position, or null if there is no tile there.
    /// </summary>
    /// <param name="position">x and y position of the tile on the grid</param>
    /// <returns>the ManaTile at the position, or null if there is no tile there</returns>
    public ManaTile GetTile(Vector2Int position) {
        return grid[position.x, position.y];
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
    /// Clear the tile at the given position, if there is one there.
    /// </summary>
    /// <param name="position"></param>
    /// <param name=""></param>
    public void ClearTile(Vector2Int position) {
        ManaTile tile = grid[position.x, position.y];
        grid[position.x, position.y] = null;
        Destroy(tile.gameObject);
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

    /// <summary>
    /// Perform gravity on all tiles.
    /// Should be called after many tiles on the board are updated at once, i.e. spellcasts.
    /// </summary>
    public void AllTileGravity() {
        // I'ts important to loop from lowest to highset y value here, so that tiles can correctly fall onto the tiles below them
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileGravity(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// The tiles parent will be hidden until the game starts.
    /// This way if other clients have slightly offset countdowns, their piece movements can still be evaluated locally
    /// but this client won't see it.
    /// </summary>
    public void ShowTiles() {
        _manaTileTransform.gameObject.SetActive(true);
    }

    public void HideTiles() {
        _manaTileTransform.gameObject.SetActive(false);
    }
}