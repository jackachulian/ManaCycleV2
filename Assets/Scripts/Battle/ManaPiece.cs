using UnityEngine;

/// <summary>
/// A collection of ManaTiles arranged into a Piece, usually a L-shaped Triomino (three connected tiles).
/// </summary>
public class ManaPiece : MonoBehaviour
{
    /// <summary>
    /// All mana tiles that are part of this piece.
    /// </summary>
    public ManaTile[] tiles {get; set;}

    /// <summary>
    /// Coordinates of the center tile of this piece.
    /// Coordinates are relative to the bottom-left corner of the board this piece is falling on.
    /// Changing position will not change the visual position of tiles, 
    /// so make sure to call UpdatePositions() once the position is valid.
    /// </summary>
    public Vector2Int position;

    /// <summary>
    /// Rotation of this piece, expressed as the number of 90-degree clockwise rotations.
    /// </summary>
    public int rotation;

    /// <summary>
    /// Moves each tile in this piece to the correct position based on position and rotation.
    /// </summary>
    public void UpdatePositions() {
        // TODO: loop through tiles and update transforms.
    }
}
