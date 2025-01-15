using System.Collections.Generic;
using UnityEngine;

public class TileUtility {
    /// <summary>
    /// Recursively expands the passed blob to all connected tiles
    /// </summary>
    /// <param name="blob">the blob (list of vecor2ints) to add to</param>
    /// <param name="position">position of the possible tile to try to add to the blob</param>
    /// <param name="color">the color to check for </param>
    /// <param name="board">the board that blob simulation is happening on. Only used for things such as board bounds, etc</param>
    /// <param name="grid">the list of tiles that blobs will be checked for on.</param>
    /// <param name="blobGrid">Contains a grid of all tiles and the blob they have been added to, or null for no blob.</param>
    public static void ExpandBlob(List<Vector2Int> blob, Vector2Int position, int color, Board board, ref ManaTile[,] tileGrid, ref List<Vector2Int>[,] blobGrid)
    {
        // Don't add to blob if the tile is in an invalid position
        if (!board.manaTileGrid.IsInBounds(position)) return;

        // Don't add to blob if already in this blob or another blob; this would cause an infinite loop
        if (tileGrid[position.x, position.y] != null) return;

        // Don't add if there is not a tile here
        ManaTile tile = tileGrid[position.x, position.y];

        if (tile == null) return;

        // Don't add if the tile is the incorrect color
        if (tile.color != color) return;

        // Add the tile to the blob and fill in its spot on the blob matrix
        blob.Add(position);
        blobGrid[position.x, position.y] = blob;

        // Expand out the current blob on all sides, checking for the same colored tile to add to this blob
        ExpandBlob(blob, position + Vector2Int.left, color, board, ref tileGrid, ref blobGrid);
        ExpandBlob(blob, position + Vector2Int.right, color, board, ref tileGrid, ref blobGrid);
        ExpandBlob(blob, position + Vector2Int.up, color, board, ref tileGrid, ref blobGrid);
        ExpandBlob(blob, position + Vector2Int.down, color, board, ref tileGrid, ref blobGrid);
    }

    /// <summary>
    /// Perform gravity on the tile at the given position on the passed tile grid.
    /// </summary>
    /// <param name="position">the position of the tile on the grid to fall</param>
    public static void TileGravity(Vector2Int position, ref ManaTile[,] tileGrid, bool animateFall = false) {
        // if null, no tile here
        ManaTile tile = tileGrid[position.x, position.y];
        if (tile == null) return;

        // While the tile is above the bottom of the board, keep falling
        while (position.y > 0) {
            // If the tile below is not empty, tile cannot fall here
            if (tileGrid[position.x, position.y - 1] != null) {
                break;
            }

            // Fall by one tile
            tileGrid[position.x, position.y] = null;
            position.y -= 1;
            tileGrid[position.x, position.y] = tile;
        }

        // if tile fell at all, animate its fall position to the new position
        if (position.y != tile.position.y) {
            tile.SetPosition(position, animateFall);
        }
    }
}