using Unity.Collections;
using UnityEngine;

/// <summary>
/// A collection of ManaTiles arranged into a Piece, usually a L-shaped Triomino (three connected tiles).
/// </summary>
public class ManaPiece : MonoBehaviour
{
    /// <summary>
    /// All mana tiles that are part of this piece.
    /// </summary>
    public ManaTile[] tiles;

    /// <summary>
    /// Coordinates of the center tile of this piece.
    /// Coordinates are relative to the bottom-left corner of the board this piece is falling on.
    /// Changing position will not change the visual position of tiles, 
    /// so make sure to call UpdatePositions() once the position is valid!
    /// </summary>
    public Vector2Int position;

    /// <summary>
    /// Rotation of this piece, expressed as the number of 90-degree clockwise rotations.
    /// </summary>
    public int rotation;

    /// <summary>
    /// Moves each tile in this piece to the correct position based on position and rotation.
    /// </summary>
    public void UpdateVisualPositions() {
        transform.localPosition = new Vector3(position.x, position.y);

        for (int i = 0; i < tiles.Length; i++) {
            ManaTile tile = tiles[i];
            Vector2Int tilePosition = GetPieceTilePosition(i);
            tile.transform.localPosition = new Vector2(tilePosition.x, tilePosition.y);
        }
    }

    /// <summary>
    /// Returns the piece-relative position of the tile of the given index relative to this piece's center tile AFTER applying rotation.
    /// </summary>
    /// <param name="index">the index of the tile in this object's <c>tiles</c> array</param>
    /// <returns>the piece-relative position of the tile at the given index</returns>
    public Vector2Int GetPieceTilePosition(int index) {
        ManaTile tile = tiles[index];

        switch(rotation) {
            case 1: // 90 degrees
                return new Vector2Int(tile.position.y, -tile.position.x);
            case 2: // 180 degrees
                return new Vector2Int(-tile.position.x, -tile.position.y);
            case 3: // 270 degrees
                return new Vector2Int(-tile.position.y, tile.position.x);
            default: // 0 degrees
                return new Vector2Int(tile.position.x, tile.position.y);
        }
    }
}
