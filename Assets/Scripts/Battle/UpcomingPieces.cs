using System.Collections.Generic;
using UnityEngine;

public class UpcomingPieces : MonoBehaviour
{
    [SerializeField] private Transform nextPieceTransform;
    [SerializeField] private Transform upcomingPiecesTransform;
    [SerializeField] private int pieceCount = 3;
    [SerializeField] private float nextPieceScale = 1.0f;
    [SerializeField] private float upcomingPiecesScale = 0.75f;
    [SerializeField] private float upcomingPiecesHeight = 2.5f;

    /// <summary>
    /// Board this upcoming piece list is for. Set on battle initialization.
    /// </summary>
    private Board board;

    /// <summary>
    /// RNG instance used to determine the colors.
    /// Seed is set on battle initialization and a new instance with that seed is created.
    /// </summary>
    private System.Random rng;

    /// <summary>
    /// All mana pieces currently in the upcoming pieces list.
    /// First element = the piece that will be spawned next on the board.
    /// Last element = the piece that was most recently enqueued
    /// </summary>
    private List<ManaPiece> manaPieces;

    /// <summary>
    /// Called when the battle is initialized.
    /// Initialized after the PieceManager, since this relies on its RNG value.
    /// Fill out the queue with pieces.
    /// These pieces will only be shown to the players once the countdown starts.
    /// </summary>
    public void InitializeBattle(Board board, int seed) {
        this.board = board;

        rng = new System.Random(seed);

        manaPieces = new List<ManaPiece>(pieceCount);
        for (int i = 0; i < pieceCount; i++) {
            SpawnNewPiece();
        }
    }

    /// <summary>
    /// Spawn a new piece and set its colors according to RNG and battle data, then add it to the manaPieces list.
    /// Does not change any UI spacing; update the UI with UpdatePieceListUI().
    /// </summary>
    private void SpawnNewPiece() {
        var piece = BattleManager.Instance.SpawnPiece();

        for (int i = 0; i < piece.tiles.Length; i++) {
            int color = rng.Next(GameManager.Instance.battleData.cycleUniqueColors);
            piece.tiles[i].SetColor(color, false, BattleManager.Instance.cosmetics);
        }

        manaPieces.Add(piece);
    }

    public ManaPiece PopNextPiece() {
        ManaPiece nextPiece = manaPieces[0];
        manaPieces.RemoveAt(0);

        // Spawn a new piece at the start to fill the empty slot
        SpawnNewPiece();

        return nextPiece;
    }

    /// <summary>
    /// Update the positions of each queued piece.
    /// </summary>
    public void UpdatePieceListUI() {
        for (int i = 0; i < manaPieces.Count; i++) {
            ManaPiece piece = manaPieces[i];

            // Parent next piece to the nextPiece parent
            if (i == 0) {
                piece.transform.SetParent(nextPieceTransform);

                // this will allow the piece's full bounds to be centered instead of the right and top mana sticking out
                piece.transform.localPosition = new Vector2(-0.5f*nextPieceScale, -0.5f*nextPieceScale);
                piece.transform.localScale = new Vector2(nextPieceScale, nextPieceScale);
            }

            // Parent upcoming pieces to the upcomingPiece parent and space them out accordingly
            else {
                float y = Mathf.Lerp(upcomingPiecesHeight*0.5f, upcomingPiecesHeight*-0.5f, (i-1) / (pieceCount-2));
                piece.transform.SetParent(upcomingPiecesTransform);
                piece.transform.localPosition = new Vector2(-0.5f*upcomingPiecesScale, y - 0.5f*upcomingPiecesScale);
                piece.transform.localScale = new Vector2(upcomingPiecesScale, upcomingPiecesScale);
            }
        }
    }

    public void ShowPieces() {
        nextPieceTransform.gameObject.SetActive(true);
        upcomingPiecesTransform.gameObject.SetActive(true);
    }

    public void HidePieces() {
        nextPieceTransform.gameObject.SetActive(false);
        upcomingPiecesTransform.gameObject.SetActive(false);
    }
}
