using System;
using Unity.Netcode;
using UnityEngine;
using Audio;

/// <summary>
/// Manages the piece falling on this board.
/// Responsibilities include piece movement, spawning new randomized pieces, falling, etc.
/// </summary>
public class PieceManager : MonoBehaviour {
    /// <summary>
    /// Invoked when a new piece is spawned onto the board as the currentPiece.
    /// </summary>
    public event Action<Board> onPieceSpawned;

    /// <summary>
    /// Invoked when the current piece is successfully moved OR rotated.
    /// </summary>
    public event Action<Board> onPieceMoved;

    /// <summary>
    /// Invoked when a new piece is placed onto the board's tile grid.
    /// </summary>
    public event Action<Board> onPiecePlaced;

    /// <summary>
    /// The amount of rows the piece should fall per second while not quickfalling.
    /// </summary>
    [SerializeField] private float fallFrequency = 1.2f;

    /// <summary>
    /// The amount of rows the piece should fall per second while quickfalling.
    /// </summary>
    [SerializeField] private float quickFallFrequency = 8f;

    /// <summary>
    /// Current falling piece on this board.
    /// </summary>
    public ManaPiece currentPiece {get; private set;}

    /// <summary>
    /// Incrementing value that tracks the percentage of time before a fall should happen.
    /// When greater than 1, subtract 1 and move the current piece down one tile.
    /// The rate this value is incremented is calculated in GetCurrentFallFrequency().
    /// </summary>
    private float movementTimer;

    /// <summary>
    /// Set to true while quickfalling and false when not.
    /// </summary>
    private bool quickfall = false;

    /// <summary>
    /// The Board of which this is managing the piece movmeent and placement. Cached on InitializeBattle()
    /// </summary>
    private Board board;

    /// <summary>
    /// Initialize the battle.
    /// Is initialized after UpcomingPieceList so that the first piece can be spawned on initialization.
    /// (The piece is spawned before the timer starts, in case boards' countdowns start at slightly different times on different clients)
    /// </summary>
    public void InitializeBattle(Board board) {
        this.board = board;

        SpawnNextPiece();
    }

    // Update is called once per frame
    void Update()
    {
        if (!board || !board.boardActive) return;

        PieceFallingUpdate();
    }

    void PieceFallingUpdate() {
        // Return if there is no player assigned or the player assigned is not owned by the local client.
        // Only the local client should handle the timing of piece falling based on inputs 
        // and then send that to the other clients.
        if (!board.player || !board.player.IsOwner) return;

        // use quickfall speed if quick falling, or normal fall frequency otherwise
        float currentFallFrequency = GetCurrentFallFrequency();

        // If there is a current piece, have it fall after delay.
        if (currentPiece != null) {
            movementTimer += BattleManager.deltaTime * currentFallFrequency;

            int iters = 0;
            while (movementTimer > 0) {
                movementTimer -= 1;

                // TODO: add a little bit of buffer time before placing the piece to allow sliding
                bool moved = TryMovePiece(Vector2Int.down);
                if (!moved) {
                    if (board.player) board.player.boardNetworkBehaviour.PlaceCurrentPieceRpc();
                }

                iters++;
                if (iters > 100) {
                    Debug.LogWarning("Tried to fall current piece over 100 times in one frame... consider lowering fall speed!");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Get the current fall delay, after accounting for quickfall and anything else that affects fall delay.
    /// (Should only be used on the owner board)
    /// </summary>
    /// <returns>current amount of delay in seconds between piece falls</returns>
    float GetCurrentFallFrequency() {
        if (quickfall) {
            return quickFallFrequency;
        } else {
            return fallFrequency;
        }
    }

    /// <summary>
    /// (Rpc Target) Update the position and rotation of the current piece being dropped.
    /// </summary>
    public void UpdateCurrentPiece(Vector2Int position, int rotation) {
        var prevPosition = position;
        var prevRotation = rotation;

        board.ghostPieceManager.UnglowAllTiles();

        currentPiece.position = position;
        currentPiece.rotation = rotation;
        currentPiece.UpdateVisualPositions();

        onPieceMoved?.Invoke(board);

        board.ghostPieceManager.UpdateGhostPiece();

        if (position != prevPosition) {
            var offset = position - prevPosition;
            AudioManager.Instance.PlayBoardSound(
                offset == Vector2Int.down ? "fall" : "move", 
                volumeScale: offset == Vector2Int.down ? 0.15f : 0.5f
            );
        } else if (prevRotation != rotation) {
            AudioManager.Instance.PlayBoardSound("rotate", volumeScale: 0.5f);
        }
    }

    /// <summary>
    /// Try to move the current piece. 
    /// If the movement would cause the piece to be in an invalid placement, return false and undo the transformation.
    /// Otherwise, returns true and updates the visual position of the tiles in the piece.
    /// </summary>
    /// <param name="offset">the offset of the piece, in board grid space</param>
    /// <returns>true if the tile was able to move.</returns>
    public bool TryMovePiece(Vector2Int offset) {
        if (!board.boardActive) return false;

        currentPiece.position += offset;

        if (!IsValidPlacement(currentPiece)) {
            currentPiece.position -= offset;
            return false;
        }

        if (offset.x != 0)
        {
            board.ghostPieceManager.UnglowAllTiles();
        }

        AudioManager.Instance.PlayBoardSound(
            offset == Vector2Int.down ? "fall" : "move", 
            volumeScale: offset == Vector2Int.down ? 0.15f : 0.5f
        );

        board.player.boardNetworkBehaviour.UpdateCurrentPieceRpc(currentPiece.position, currentPiece.rotation);

        return true;
    }

    /// <summary>
    /// Try to rotate the current piece. 
    /// If the rotation would cause the piece to be in an invalid rotation, try to nudge it right, left and up.
    /// If no nudges are possible, undo the rotation and return false.
    /// </summary>
    /// <param name="rotation">amount of clockwise rotations. negative is counter-clockwise rotations</param>
    /// <returns>true if the piece was successfully rotated</returns>
    public bool TryRotatePiece(int rotations) {
        if (!board.boardActive) return false;

        if (rotations == 0) {
            Debug.LogWarning("Piece was rotated zero times");
            return true;
        }

        int initialRotation = currentPiece.rotation;
        currentPiece.rotation = (initialRotation + 4 + rotations) % 4;

        if (!IsValidPlacement(currentPiece)) {
            // If clockwise, try to nudge right, left then up.
            if (rotations > 0) {
                if (!TryMovePiece(Vector2Int.right) && !TryMovePiece(Vector2Int.left) && !TryMovePiece(Vector2Int.up)) {
                    currentPiece.rotation = initialRotation;
                    return false;
                }
            }

            // If clockwise, try to nudge left, right then up.
            else if (rotations < 0) {
                if (!TryMovePiece(Vector2Int.left) && !TryMovePiece(Vector2Int.right) && !TryMovePiece(Vector2Int.up)) {
                    currentPiece.rotation = initialRotation;
                    return false;
                }
            }
        }

        AudioManager.Instance.PlayBoardSound("rotate", volumeScale: 0.5f);

        board.player.boardNetworkBehaviour.UpdateCurrentPieceRpc(currentPiece.position, currentPiece.rotation);
        return true;
    }

    /// <summary>
    /// Set quickfalling to either true or false.
    /// </summary>
    /// <param name="newQuickFall">the new quickfall value, true = quickfalling, false = not quickfalling</param>
    public void SetQuickfall(bool newQuickFall) {
        quickfall = newQuickFall;
    }

    /// <summary>
    /// Returns true if all tiles in the piece are within bounds and not obstructed by any tiles already placed.
    /// </summary>
    bool IsValidPlacement(ManaPiece piece) {
        for (int i = 0; i < piece.tiles.Length; i++) {
            Vector2Int boardPosition = piece.position + piece.GetPieceTilePosition(i);

            // If any tile in the piece is out of bounds return false
            if (!board.manaTileGrid.IsInBounds(boardPosition)) {
                return false;
            }

            // If any tile in the piece intersects another tile, return false
            // (If value is not null, there is a tile there)
            if (board.manaTileGrid.HasTile(boardPosition)) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// (Rpc Target) Place the current piece and spawn the next piece.
    /// Check for topout and defeat if so.
    /// </summary>
    public void PlaceCurrentPiece() {
        PlacePiece(currentPiece);
        onPiecePlaced?.Invoke(board);

        board.ghostPieceManager.DestroyGhostPiece();
        currentPiece = null;

        board.healthManager.AdvanceDamageQueue();
        AudioManager.Instance.PlayBoardSound("place", volumeScale: 0.5f);

        SpawnNextPiece();


        // If the newly spawned piece is in an invalid position, player has topped out
        if (!IsValidPlacement(currentPiece)) {
            Destroy(currentPiece.gameObject);
            board.ghostPieceManager.DestroyGhostPiece();
            board.Defeat();
        }
    }

    /// <summary>
    /// /// Move all tiles of the piece onto this board's grid of tiles and destroy the piece container afterwards.
    /// Uses the piece's <c>GetTilePosition()</c> to get the positions to place each tile.
    /// </summary>
    /// <param name="piece">the piece to place</param>
    /// <param name="spawnNewPieceAfter">whether or not a new piece should be spawned immediately after the current one is placed</param>
    void PlacePiece(ManaPiece piece) {
        // Place pieces' tiles on the mana tile grid based on the piece position and orientation
        Vector2Int[] placePositions = piece.PlaceTilesOnBoard(board);

        // Destroy piece container that is no longer needed
        Destroy(piece.gameObject);

        // Perform gravity on all placed tiles - lower Y position tiles should fall first so tiles above correctly fall on top of them
        Array.Sort(placePositions, (pos1, pos2) => pos1.y - pos2.y);
        foreach (Vector2Int pos in placePositions) {
            board.manaTileGrid.TileGravity(pos);
        }

        // Let spellcastmanager know the board state has changed and to rebuild all blobs
        board.spellcastManager.RefreshBlobs();
    }

    /// <summary>
    /// Is called after the current piece is placed.
    /// Replaces the current piece with the next piece from the upcomingpieces list.
    /// </summary>
    public void SpawnNextPiece() {
        // TODO: grab the new piece from the upcoming pieces list
        var nextPiece = board.upcomingPieces.PopNextPiece();
        SpawnPiece(nextPiece);
    }

    /// <summary>
    /// Make the piece the current piece and start dropping it on the board.
    /// </summary>
    /// <param name="piece"></param>
    public void SpawnPiece(ManaPiece piece) {
        if (currentPiece != null) {
            Debug.LogWarning("A piece was spawned while another piece was currently spawned. Make sure you destroy the old piece first!");
        }

        currentPiece = piece;

        // parent the next piece onto the board so the player can see it
        currentPiece.transform.SetParent(board.manaTileGrid.manaTileTransform);
        currentPiece.transform.localScale = Vector3.one;

        // spawn position will be the top row, middle column of the board
        Vector2Int spawnPos = new Vector2Int(board.manaTileGrid.width / 2, board.manaTileGrid.visual_height-1); 
        currentPiece.position = spawnPos;

        board.ghostPieceManager.UnglowAllTiles();

        currentPiece.UpdateVisualPositions();

        // TODO: listen for onPieceSpawned in ghostPieceManager instead of calling here
        board.ghostPieceManager.CreateGhostPiece();

        // TODO: listen for onPieceSpawned in upcomingPieces instead of calling here
        board.upcomingPieces.UpdatePieceListUI();

        onPieceSpawned?.Invoke(board);
    }

    /// <summary>
    /// Will destroy / discard the current piece, and replace it with the passed piece.
    /// </summary>
    public void ReplaceCurrentPiece(ManaPiece piece) {
        Destroy(currentPiece.gameObject);
        board.ghostPieceManager.DestroyGhostPiece();
        currentPiece = null;
        SpawnPiece(piece);
    }
}