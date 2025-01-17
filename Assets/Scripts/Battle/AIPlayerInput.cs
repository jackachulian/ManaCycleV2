using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles a PlayerInput's messages for the battle scene, controlling the board that is assigned.
/// </summary>
public class AIPlayerInput : MonoBehaviour
{
    /// <summary>
    /// The board this AI player is controlling.
    /// </summary>
    private Board board;

    [SerializeField] private float minDecisionTime = 0.3f;
    [SerializeField] private float maxDecisionTime = 0.9f;

    /// <summary>
    /// When reaching 0, player will make a decision.
    /// </summary>
    private float timeUntilNextDecision;

    /// <summary>
    /// Current rotation direction being used to reach the target rotation.
    /// -1 = counter-clockwise
    /// 0 = not rotating
    /// 1 = clockwise
    /// </summary>
    private int rotationDirection = 0;

    /// <summary>
    /// AI will try to match this rotation before moving / quickfalling the piece.
    /// -1 means not decided yet.
    /// </summary>
    private int targetRotation = -1;

    /// <summary>
    /// AI will try to match this column before quickfalling the piece.
    /// -1 means not decided yet.
    /// </summary>
    private int targetCol = -1;

    /// <summary>
    /// If true, player will spellcast on their next decision.
    /// </summary>
    private bool spellcastNext = false;

    void Update() {
        timeUntilNextDecision -= Time.deltaTime;
        if (timeUntilNextDecision < 0) {
            timeUntilNextDecision += Random.Range(minDecisionTime, maxDecisionTime);
            MakeDecision();
        }
    }

    public void AssignBoard(Board board) {
        if (this.board == board) return;

        if (this.board != null) {
            UnassignBoard();
        }

        this.board = board;
        if (board) {
            board.pieceManager.onPieceSpawned += OnPieceSpawned;
            board.pieceManager.onPiecePlaced += OnPiecePlaced;
        }
    }

    public void UnassignBoard() {
        if (!board) return;

        board.pieceManager.onPieceSpawned -= OnPieceSpawned;
        board.pieceManager.onPiecePlaced -= OnPiecePlaced;
    }

    /// <summary>
    /// Decide the next action
    /// Decisions the AI will make:
    /// -rotate left or right if current piece is not in targetRotation.
    /// -move piece left or right if current piece is not in targetCol.
    /// -start quickfalling if piece is in targetRow and targetRotation.
    /// </summary>
    void MakeDecision() {
        ManaPiece currentPiece = board.pieceManager.currentPiece;

        // if spellcastNext is true, spellcast as the decision
        if (spellcastNext) {
            spellcastNext = false;
            board.spellcastManager.TrySpellcast();
        }

        // Try to rotate the piece if not already in target rotation.
        else if (currentPiece.rotation != targetRotation) {
            bool rotated = board.pieceManager.TryRotatePiece(rotationDirection);

            // If the tile could be rotated, this counts as the decision
            if (rotated) {

            } 
            // if not, piece can't reach the target placement, decide a new placement
            else {
                DecidePlacement();
            }
        }

        // Try to move the piece if it is not already in the target column.
        else if (currentPiece.position.x != targetCol) {
            var moveDir = currentPiece.position.x > targetCol ? Vector2Int.left : Vector2Int.right;
            bool moved = board.pieceManager.TryMovePiece(moveDir);

            // If the tile could be moved, this counts as the decision
            if (moved) {
                
            }
            // if not, piece cannot reach the target placement, decide a new placement
            else {
                DecidePlacement();
            }
        }

        // If piece is in the target rotation and column, start quickfalling if not already
        else {
            board.pieceManager.SetQuickfall(true);
        }
    }

    /// <summary>
    /// Scan the board and choose an optimal column and rotation to place the piece in.
    /// </summary>
    void DecidePlacement() {
        // TODO: make placements actually intelligent. It is random for now

        // this can stay random, unless the AI should always rotate the shortest amount of rotations
        rotationDirection = Random.value > 0.5 ? 1 : -1; 
        targetRotation = Random.Range(0, 4);
        targetCol = Random.Range(0, board.manaTileGrid.width);
    }

    /// <summary>
    /// Called when placing a piece.
    /// If current color is clearable, have a certain chance to start a spellcast.
    /// </summary>
    void DecideSpellcast() {
        if (board.spellcastManager.IsCurrentColorClearable() && Random.value < 0.3f) {
            spellcastNext = true;
        }
    }

    void OnPieceSpawned() {
        // decide the placement of a piece whenever a new piece is spawned.
        DecidePlacement();
    }

    /// <summary>
    /// Call when a piece is placed. will let the AI know to stop quickfalling and decide a new placement for the new piece.
    /// </summary>
    void OnPiecePlaced() {
        // stop quickfalling, and decide if the player should spellcast next
        board.pieceManager.SetQuickfall(false);
        DecideSpellcast();
    }

    public void OnSpellcast() {
        board.spellcastManager.TrySpellcast();
    }
}