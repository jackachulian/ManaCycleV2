using System;
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
    /// Event that should be invoked whenever the piece is (successfully) moved by a board, either by column change or quickfall.
    /// </summary>
    public event Action<ManaPiece> onVisualPositionUpdated;

    /// <summary>
    /// Event invoked when this piece is placed by a board's piecemanager, 
    /// after this piece's tiles are placed onto the board, but before this piece is destroyed.
    /// Can be listened to by ability-driven pieces, such as z?man's mini z?man.
    /// </summary>
    public event Action<ManaPiece> onPlaced;

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

        onVisualPositionUpdated?.Invoke(this);
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

    /// <summary>
    /// Place tiles onto the given board's mana tile grid.
    /// Note that this does not apply gravity.
    /// </summary>
    /// <returns>all positions a tile was placed in</returns>
    public Vector2Int[] PlaceTilesOnBoard(Board board)
    {
        Vector2Int[] placePositions = new Vector2Int[tiles.Length];

        // Convert the position space of all tiles from piece-relative to board-relative (apply position and rotation)
        // and reparent the mana tiles to this board
        for (int i = 0; i < tiles.Length; i++)
        {
            ManaTile tile = tiles[i];
            Vector2Int boardPosition = position + GetPieceTilePosition(i);
            placePositions[i] = boardPosition;
            board.manaTileGrid.PlaceTile(tile, boardPosition);
            tile.transform.SetParent(board.manaTileGrid.manaTileTransform, true);
            tile.SetBoardPosition(boardPosition, false); // no animation; fall animation will perform the animation
        }

        onPlaced?.Invoke(this);

        return placePositions;
    }
}
