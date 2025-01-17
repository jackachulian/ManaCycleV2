using Battle;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Handles the battler portrait sprite, fall anim, and any other board-specific visual elements on the board
/// </summary>
public class BoardUI : MonoBehaviour {
    [Header("General")]
    [SerializeField] private SpriteRenderer portraitSpriteRenderer;
    
    /// <summary>
    /// shows "WIN!" when winning and "LOSE" when defeated
    /// </summary>
    [SerializeField] private TMP_Text winLoseText;

    /// <summary>
    /// Shows "Match 3 ___" on failed cast
    /// TODO add mana symbol to message for accessibility
    /// </summary>
    [SerializeField] private TMP_Text guideText;

    /// <summary>
    /// Names for all the colors based on color int 
    /// TODO string name should probably be stored somewhere in cosmetic object
    /// </summary>
    private string[] colorNames = {"Red", "Green", "Blue", "Yellow", "Purple"};


    // May move defeat fall logic to its own component for better organization
    [Header("Defeat Fall")]
    [SerializeField] private Transform fallTransform;
    [SerializeField] float fallAcceleration = 50f;
    /** rotation angular speed acceleration - degrees/s^2 (starts at 0) */
    [SerializeField] float fallAngularAcceleration = 25f;
    /** Initial falling speed */
    [SerializeField] float initialFallSpeed = -60f;


    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool doPlaceAnimation;


    [Header("Particles")]
    [SerializeField] private ParticleSystem particles;
    // min max
    [SerializeField] private Vector2 particleAmount;


    [Header("Cycle Pointer")]
    /// <summary>
    /// CyclePointer to position on the current color being cleared
    /// </summary>
    [SerializeField] private Transform cyclePointer;

    /// <summary>
    /// Amount of units of offset to position the cycle pointer.
    /// </summary>
    [SerializeField] private float cyclePointerOffset = 1.5f;

    private enum BoardSide {
        LEFT,
        RIGHT,
        ABOVE,
        BELOW
    }
    /// <summary>
    /// The side of the screen the board is on. Used for cycle pointer positioning.
    /// </summary>
    [SerializeField] private BoardSide boardSide = BoardSide.LEFT;

    /// <summary>
    /// UI object that shows chain number and the duration remaining to extend the chain
    /// </summary>
    [SerializeField] private PopupUI _chainPopup;
    public PopupUI chainPopup => _chainPopup;

    /// <summary>
    /// UI object that shows cascade number and the duration remaining to extend the cascade
    /// </summary>
    [SerializeField] private PopupUI _cascadePopup;
    public PopupUI cascadePopup => _cascadePopup;

    // Falling anim variables
    Vector2 fallStartPos;
    float fallDistance, fallSpeed, rotation, angularSpeed;
    bool falling = false;

    // Board component, cached on InitializeBattle
    private Board board;

    void Start() {
        winLoseText.text = "";
        animator.enabled = true;
        particles.Stop();
    }

    public void InitializeBattle(Board board) {
        this.board = board;
        board.pieceManager.onPiecePlaced += OnPiecePlaced;
        board.manaTileGrid.TileClearedNotifier += OnTileCleared;
    }

    void Update() {
        if (falling)
        {
            fallDistance += fallSpeed*Time.smoothDeltaTime;
            fallSpeed += fallAcceleration*Time.smoothDeltaTime;
            rotation += angularSpeed*Time.smoothDeltaTime;
            angularSpeed += fallAngularAcceleration*Time.smoothDeltaTime;

            fallTransform.localPosition = fallStartPos + Vector2.down*fallDistance;
            fallTransform.eulerAngles = new Vector3(0, 0, rotation);
        }
    }

    /// <summary>
    /// Show the board.
    /// Board should be shown when there is a player controlling this board.
    /// </summary>
    public void ShowBoard() {
        Debug.Log("Showing board "+this.gameObject);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the board.
    /// Board should be hidden when the board is not being used in the current gamemode (not enough players to need it, etc).
    /// </summary>
    public void HideBoard() {
        Debug.Log("Hiding board "+this.gameObject);
        gameObject.SetActive(false);
    }

    public void RepositionCyclePointer(int cycleIndex) {
        ManaCycle cycle = board.boardManaCycle != null ? board.boardManaCycle : BattleManager.Instance.manaCycle;
        var cycleManaTile = cycle.GetCycleTile(cycleIndex);
        
        // TODO: in 2-player mode, player 2's pointer is offset to the right instead of left. 
        // in 3 and 4-player mode, half and half on each side and handle overlaps by spreading out the sprites slightly
        Vector3 offsetDirection;
        switch (boardSide) {
            case BoardSide.LEFT:
                offsetDirection = Vector3.left; break;
            case BoardSide.RIGHT:
                offsetDirection = Vector3.right; break;
            case BoardSide.ABOVE:
                offsetDirection = Vector3.up; break;
            case BoardSide.BELOW:
                offsetDirection = Vector3.down; break;
            default:
                offsetDirection = Vector3.zero; break;
        }
        cyclePointer.position = cycleManaTile.transform.position + offsetDirection * cyclePointerOffset;
    }
    
    public void StartDefeatFall() {
        if (!falling)
        {
            animator.enabled = false;
            fallStartPos = fallTransform.localPosition;
            fallDistance = 0;
            fallSpeed = initialFallSpeed;
            rotation = 0;
            angularSpeed = 0;
            falling = true;
        }
    }

    public void ShowBattler(Battler battler) {
        if (battler) {
            portraitSpriteRenderer.sprite = battler.sprite;
        } else {
            portraitSpriteRenderer.sprite = null;
        }
    }

    public void ShowWinText() {
        winLoseText.gameObject.SetActive(true);
        winLoseText.text = "Win!";
    }

    public void ShowLoseText() {
        winLoseText.gameObject.SetActive(true);
        winLoseText.text = "Lose";
    }

    public void OnPiecePlaced()
    {
        if (doPlaceAnimation) animator.Play("Place");
    }

    public void OnDamage()
    {
        animator.Play("Hurt");
    }

    public void OnSpellcast()
    {
        animator.Play("Cast");
    }

    public void OnFailSpellcast(int color)
    {
        guideText.text = string.Format("<Color={0}>Match 3 {0}!", colorNames[color]);
        animator.Play("FailCast");
    }

    public void OnTileCleared(Vector2 position, int color)
    {
        Color pieceCol = BattleManager.Instance.cosmetics.manaVisuals[color].material.GetColor("_InnerColor");
        var colModule = particles.colorOverLifetime;
        // set particle color gradient
        colModule.color = new ParticleSystem.MinMaxGradient (
            pieceCol, 
            new Color(pieceCol.r, pieceCol.g, pieceCol.b, 0.0f)
        );
        // play tile clear particles
        particles.transform.position = position;
        particles.Emit(Random.Range(1,3));
    }
}