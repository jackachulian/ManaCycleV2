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

    // ========= Fall variables ========
    /// <summary>
    /// Position being animated towards, current position will fall towards this
    /// </summary>
    private Vector2 targetPosition;

    /// <summary>
    /// Current fall speed. Accelerated over time while falling
    /// </summary>
    private float currentFallSpeed;


    [SerializeField] private float initialFallSpeed = 40f;

    [SerializeField] private float fallAcceleration = 40f;

    bool falling = false;

    void Update() {
        if (falling) {
            currentFallSpeed += fallAcceleration * Time.deltaTime;
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPosition, currentFallSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Sets the color of this mana. (both visual and game logic).
    /// </summary>
    /// <param name="color">an integer representing the color.</param>
    /// <param name="manaCosmetics">Cosmetics object of the sprites and colors to use. If null, visuals are not changed.</param>
    public void SetColor(int color, ManaCosmetics manaCosmetics = null) {
        this.color = color;

        if (manaCosmetics) {
            var manaVisual = manaCosmetics.manaVisuals[color];
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = manaVisual.sprite;
            spriteRenderer.color = manaVisual.tint;
        }
    }

    // Animate towards the new position
    public void AnimatePosition(Vector2 targetPosition) {
        // TODO: interpolate between current and target position over time
        this.targetPosition = targetPosition;
        falling = true;
        currentFallSpeed = initialFallSpeed;
    }
}