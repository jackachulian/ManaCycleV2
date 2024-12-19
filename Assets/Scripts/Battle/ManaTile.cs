using UnityEngine;

/// <summary>
/// A single mana tile. Can be placed on ManaGrids, and are part of ManaPieces.
/// </summary>
public class ManaTile : MonoBehaviour
{
    /// <summary>
    /// Position of this tile
    /// If contained within a piece, this is the offset from the center tile of the piece (BEFORE piece rotation).
    /// If contained within a board's tile grid, this is the offset from the bottom-left tile.
    /// </summary>
    public Vector2Int position;

    /// <summary>
    /// Integer representing the color used for tile clearing.
    /// (This refers to the game logic color int, not the visual color.)
    /// in standard games: 0=red, 1=green, 2=blue, 3=yellow, 4=purple
    /// </summary>
    public int color {get; private set;}


    /// <summary>
    /// Sets the color of this mana.
    /// </summary>
    /// <param name="manaCosmetics">Cosmetics object of the sprites and colors to use</param>
    /// <param name="color">an integer representing the color.</param>
    public void SetColor(ManaCosmetics manaCosmetics, int color) {
        this.color = color;

        var manaVisual = manaCosmetics.manaVisuals[color];
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = manaVisual.sprite;
        spriteRenderer.color = manaVisual.tint;
    }

    // Animate towards the new position
    public void AnimatePosition(Vector2 targetPosition) {
        // TODO: interpolate between current and target position over time
        transform.localPosition = targetPosition;
    }
}