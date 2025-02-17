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
    [SerializeField] private Vector2Int _position;
    public Vector2Int position => _position;

    /// <summary>
    /// Integer representing the color used for tile clearing.
    /// (This refers to the game logic color int, not the visual color.)
    /// in standard games: -1=chrome/white (all colors), 0=red, 1=green, 2=blue, 3=yellow, 4=purple
    /// </summary>
    [SerializeField] private int _color;
    public int color => _color;

    public bool isGhost { get; private set; } = false;
    public bool isPulseGlowing { get; private set; } = false;
    public bool isFadeGlowing { get; private set; } = false;


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
            currentFallSpeed += fallAcceleration * BattleManager.deltaTime;
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPosition, currentFallSpeed * BattleManager.deltaTime);
        }
    }

    /// <summary>
    /// Sets the color of this mana. (both visual and game logic).
    /// </summary>
    /// <param name="color">an integer representing the color.</param>
    /// <param name="manaCosmetics">Cosmetics object of the sprites and colors to use. If null, visuals are not changed.</param>
    public void SetColor(int color) {
        _color = color;
    }

    public void SetGhost(bool isGhost)
    {
        this.isGhost = isGhost;
    }

    public void SetPulseGlow(bool isPulseGlow)
    {
        this.isPulseGlowing = isPulseGlow;
    }

    public void SetFadeGlow(bool isFadeGlow)
    {
        this.isFadeGlowing = isFadeGlow;
    }

    /// <summary>
    /// Updates ALL things about how this mana looks based on all its current properties.
    /// </summary>
    /// <param name="manaCosmetics">cosmetics to get materials from. defaults to BattleManager instance's cosmetics if not provided</param>
    /// <param name="board">The board. Only needed if this tile will be fade glowed, which is most tiles within a battle.</param>
    public void UpdateVisuals(ManaCosmetics manaCosmetics = null, Board board = null)
    {
        // if no mana cosmetics passed in parameters, default to the battlemanager's mana cosmetics
        if (!manaCosmetics) manaCosmetics = BattleManager.Instance.cosmetics;
        var renderer = GetComponent<Renderer>();

        if (!renderer) return;

        if (isFadeGlowing)
        {
            renderer.material = color == -1 ? board.chromeFadeGlowMaterial : board.fadeGlowMaterials[color];
        }
        else
        {
            ManaVisual manaVisual;
            if (color == -1)
            {
                manaVisual = isPulseGlowing ? BattleManager.Instance.chromePulseGlowManaVisual : manaCosmetics.chromeManaVisual;
                
            } else {
                manaVisual = isPulseGlowing ? BattleManager.Instance.pulseGlowManaVisuals[color] : manaCosmetics.manaVisuals[color];
            }
            renderer.material = isGhost ? manaVisual.ghostMaterial : manaVisual.material;
        }

        renderer.sortingOrder = isGhost ? -1 : 0; // draw ghost tiles behind regular tiles
    }

    // Set position of the tile. if animate is true, a falling animation will occur bewteen the previous and current tile position.
    // Note: the visual position will vary based on whether or not this is currently parented under either a piece or the board!
    // THis method should only be used when a piece has already been placed on the board, not while it is currently contained in a piece.
    public void SetBoardPosition(Vector2Int position, bool animate) {
        _position = position;
        targetPosition = (Vector2)position;

        if (animate) {
            falling = true;
            currentFallSpeed = initialFallSpeed;
        } else {
            falling = false;
            transform.localPosition = targetPosition;
        }
    }
}