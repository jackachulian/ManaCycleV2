using System.Collections.Generic;
using System.Linq;
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
    /// Used by the AI to simulate tile placement.
    /// </summary>
    private ManaTile[,] simulatedTileGrid;

    /// <summary>
    /// Used by the AI to connected tiles.
    /// </summary>
    private List<Vector2Int>[,] simulatedBlobGrid;

    private int[] placementCheckOrder;

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
        if (!board) return;

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

        simulatedTileGrid = new ManaTile[board.manaTileGrid.width, board.manaTileGrid.height];
        simulatedBlobGrid = new List<Vector2Int>[board.manaTileGrid.width, board.manaTileGrid.height];

        // this wll be reshuffled each decision
        placementCheckOrder = new int[board.manaTileGrid.width * 4];
        for (int i = 0; i < placementCheckOrder.Length; i++) {
            placementCheckOrder[i] = i;
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
        // Store the reference to this piece.
        // The rotation property of this will be temporarily changed to aid with faster calculation,
        // make sure it is reset afterwards!
        ManaPiece piece = board.pieceManager.currentPiece;
        int restoreRotation = piece.rotation;

        // Will match the tile fall positions of each tile in order for the current placement being tested
        Vector2Int[] placePositions = new Vector2Int[piece.tiles.Length];
        System.Array.Fill(placePositions, -Vector2Int.one);

        // this can stay random, unless the AI should always rotate the shortest amount of rotations
        rotationDirection = Random.value > 0.5 ? 1 : -1; 

        // start off with random column and rotation
        // these will be overridden whenever the score of a placement exceeds highestPlacementScore
        targetCol = Random.Range(0, board.manaTileGrid.width);
        targetRotation = Random.Range(0, 4);
        int highestPlacementScore = 0;

        // Copy the latest state of the board before using it for calculations
        System.Array.Copy(board.manaTileGrid.tileGrid, simulatedTileGrid, board.manaTileGrid.width * board.manaTileGrid.height);

        // Shuffle the list so that placements can be checked in a random order to prevent column bias
        placementCheckOrder = placementCheckOrder.OrderBy(x => Random.value).ToArray();

        // Loop through all possible drop columns for the piece
        for (int placementIndex = 0; placementIndex < placementCheckOrder.Length; placementIndex++) {
            int placement = placementCheckOrder[placementIndex];
            int col = placement / 4;
            int rot = placement % 4;

            Vector2Int pos = new Vector2Int(col, piece.position.y);

            // Set rotation of the actual piece. Remember to undo after the decision is made!
            piece.rotation = rot;

            // will be set to false if any tile causes this to be an invalid placement (ex. OOB tile, blocks piece spawn point)
            bool validPlacement = true;

            // Convert the position space of all tiles from piece-relative to board-relative (apply position and rotation).
            // Create ghost tiles at the location of the piece on the board
            for (int i = 0; i < piece.tiles.Length; i++) {
                // We're just grabbing the position of the actual tile that exists on the board
                Vector2Int boardPosition = pos + piece.GetPieceTilePosition(i);

                // If any tile in the piece is out of bounds return false
                if (!board.manaTileGrid.IsInBounds(boardPosition)) {
                    validPlacement = false; break;
                }

                placePositions[i] = boardPosition;

                // place on the simulated grid so that gravity can pull it down
                simulatedTileGrid[boardPosition.x, boardPosition.y] = piece.tiles[i];
            }

            // Only perform gravity and check for connections if tile is not in an invalid position
            if (validPlacement) {
                // Keep track of a placement score that should be incremented based on how preferable this placement is
                int placementScore = 0;

                // Perform simulated gravity on all simulated tiles, updating the place positions
                System.Array.Sort(placePositions, (pos1, pos2) => pos1.y - pos2.y);
                for (int i = 0; i < placePositions.Length; i++) {
                    Vector2Int placePos = placePositions[i];
                    int fallRow = TileUtility.TileGravity(placePos, ref simulatedTileGrid, false, false);
                    placePos = new Vector2Int(placePos.x, fallRow);
                    placePositions[i] = placePos;
                }

                // Reset the blob array to empty
                for (int y = 0; y < board.manaTileGrid.height; y++) {
                    for (int x = 0; x < board.manaTileGrid.width; x++) {
                        simulatedBlobGrid[x,y] = null;
                    }
                }

                // Rebuild all blobs based on the state of the simulated tile grid
                // Only build blobs off tiles that are connected to a simulated placed tile
                for (int i = 0; i < placePositions.Length; i++) {
                    ManaTile tile = piece.tiles[i];
                    Vector2Int placePos = placePositions[i];

                    // If this ghost tile has already been added to a blob (ghost tile blob connected to itself), skip this ghost tile
                    List<Vector2Int> blob = simulatedBlobGrid[placePos.x, placePos.y];
                    if (blob != null)
                    {
                        continue;
                    };

                    // Try building a blob off this ghost tile
                    blob = new List<Vector2Int>();
                    TileUtility.ExpandBlob(ref blob, placePos, tile.color, board, ref simulatedTileGrid, ref simulatedBlobGrid);

                    // if blob is clearable, glow all the connected mana
                    // Gain 1 "score" point for each connected tile, even if it doesn't meet required blob size,
                    // will still help with build blobs to get same color together
                    placementScore += blob.Count;
                }

                Debug.Log("col="+col+", rot="+rot+", placementScore="+placementScore);

                // If score exceeds highest placement score, use this as the preferred placement
                if (placementScore > highestPlacementScore) {
                    highestPlacementScore = placementScore;
                    targetCol = col;
                    targetRotation = rot;
                }
            }

            // Clear place positions / sim tile grid so that next piece calculation can be made at the next rotation/col combination
            for (int i = 0; i < placePositions.Length; i++) {
                Vector2Int placePos = placePositions[i];
                // only clear the tiles that are needed to be cleared, aka the only ones in the simulated placement
                // this is to prevent doing the possibly expensive array copy for each of the iterations and instead just reset the changed tiles
                // ignore OOB tiles, these are the (-1, -1)'s set as initial values incase placement is invalid while checking placement
                if (board.manaTileGrid.IsInBounds(placePos)) {
                    simulatedTileGrid[placePos.x, placePos.y] = null;
                }
            }
        }

        // restore the initial rotation of the piece that may have been changed during the calculation
        piece.rotation = restoreRotation;

        Debug.Log("Target col and rotation decided. targetCol="+targetCol+", targetRotation="+targetRotation+", placementScore="+highestPlacementScore);
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
}