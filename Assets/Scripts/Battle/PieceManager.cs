using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the piece falling on this board.
/// Responsibilities include piece movement, spawning new randomized pieces, falling, etc.
/// </summary>
public class PieceManager : NetworkBehaviour {
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
    private ManaPiece currentPiece;

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
    /// RNG instance used to determine the colors.
    /// Seed is set on battle initialization.
    /// </summary>
    private System.Random rng;

    /// <summary>
    /// The Board this is managing the spellcasts of. Cached on InitializeBattle()
    /// </summary>
    private Board board;

    public void InitializeBattle(Board board, int seed) {
        this.board = board;

        rng = new System.Random(seed);

        SpawnNewPiece();
    }

    // Update is called once per frame
    void Update()
    {
        // Only perform fall logic if this board is owned
        if (!IsOwner) return;

        // use quickfall speed if quick falling, or normal fall frequency otherwise
        float currentFallFrequency = GetCurrentFallFrequency();

        // If there is a current piece, have it fall after delay.
        if (currentPiece != null) {
            movementTimer += Time.deltaTime * currentFallFrequency;

            int iters = 0;
            while (movementTimer > 0) {
                movementTimer -= 1;

                // TODO: add a little bit of buffer time before placing the piece to allow sliding
                bool moved = TryMovePiece(Vector2Int.down);
                if (!moved) {
                    PlaceCurrentPieceRpc();
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
        if (!IsOwner) {
            Debug.Log("Trying to get fall speed on a non-owner client! Ony the owner should handle piece falling");
            return 0;
        }

        if (quickfall) {
            return quickFallFrequency;
        } else {
            return fallFrequency;
        }
    }

    /// <summary>
    /// RPC to send to other clients when the position of the piece successfully changes.
    /// </summary>
    [Rpc(SendTo.NotOwner, RequireOwnership = true)]
    public void UpdateCurrentPieceRpc(Vector2Int position, int rotation) {
        currentPiece.position = position;
        currentPiece.rotation = rotation;
        currentPiece.UpdateVisualPositions();
    }

    /// <summary>
    /// Try to move the current piece. 
    /// If the movement would cause the piece to be in an invalid placement, return false and undo the transformation.
    /// Otherwise, returns true and updates the visual position of the tiles in the piece.
    /// </summary>
    /// <param name="offset">the offset of the piece, in board grid space</param>
    /// <returns>true if the tile was able to move.</returns>
    public bool TryMovePiece(Vector2Int offset) {
        currentPiece.position += offset;

        if (!IsValidPlacement(currentPiece)) {
            currentPiece.position -= offset;
            return false;
        }

        UpdateCurrentPieceRpc(currentPiece.position, currentPiece.rotation);
        currentPiece.UpdateVisualPositions();
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

        UpdateCurrentPieceRpc(currentPiece.position, currentPiece.rotation);
        currentPiece.UpdateVisualPositions();
        return true;
    }

    /// <summary>
    /// Set quickfalling to either true or false.
    /// </summary>
    /// <param name="newQuickFall">the new quickfall value, true = quickfalling, false = not quickfalling</param>
    public void SetQuickfall(bool newQuickFall) {
        // Only the client's board should manage its own quickfall.
        // In fact, other clients don't even need to know if the current piece is quickfalling, because *all* movements are currently sent,
        // including the moving down that happens when the client manages its own piece falling
        if (!IsOwner) {
            Debug.Log("Only the owner of a board should manage its own quickfall.");
            return;
        }

        quickfall = newQuickFall;
    }

    /// <summary>
    /// Returns true if all tiles in the piece are within bounds and not obstructed by any tiles already placed.
    /// </summary>
    bool IsValidPlacement(ManaPiece piece) {
        for (int i = 0; i < piece.tiles.Length; i++) {
            Vector2Int piecePosition = piece.position + piece.GetTilePosition(i);

            // If any tile in the piece is out of bounds return false
            if (!board.manaTileGrid.IsInBounds(piecePosition)) {
                return false;
            }

            // If any tile in the piece intersects another tile, return false
            // (If value is not null, there is a tile there)
            if (board.manaTileGrid.HasTile(piecePosition)) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// RPC to place the current piece and spawn the next one.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    void PlaceCurrentPieceRpc() {
        PlacePiece(currentPiece);
        board.healthManager.AdvanceDamageQueue();
        SpawnNewPiece();
    }

    /// <summary>
    /// /// Move all tiles of the piece onto this board's grid of tiles and destroy the piece container afterwards.
    /// Uses the piece's <c>GetTilePosition()</c> to get the positions to place each tile.
    /// </summary>
    /// <param name="piece">the piece to place</param>
    /// <param name="spawnNewPieceAfter">whether or not a new piece should be spawned immediately after the current one is placed</param>
    void PlacePiece(ManaPiece piece) {
        Vector2Int[] placePositions = new Vector2Int[piece.tiles.Length];

        // Convert the position space of all tiles from piece-relative to board-relative (apply position and rotation)
        // and reparent the mana tiles to this board
        for (int i = 0; i < piece.tiles.Length; i++) {
            ManaTile tile = piece.tiles[i];
            Vector2Int boardPosition = piece.position + piece.GetTilePosition(i);
            tile.position = boardPosition;
            placePositions[i] = boardPosition;
            board.manaTileGrid.PlaceTile(tile, boardPosition);
            tile.transform.SetParent(board.manaTileGrid.manaTileTransform, true);
        }

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
    /// </summary>
    public void SpawnNewPiece() {
        currentPiece = board.battleManager.SpawnPiece();
        currentPiece.transform.SetParent(board.manaTileGrid.manaTileTransform);

        // spawn position will be the top row, middle column
        Vector2Int spawnPos = new Vector2Int(board.manaTileGrid.width / 2, board.manaTileGrid.visual_height-1); 
        currentPiece.position = spawnPos;
        currentPiece.UpdateVisualPositions();

        for (int i = 0; i < currentPiece.tiles.Length; i++) {
            int color = rng.Next(5);
            currentPiece.tiles[i].SetColor(color, board.battleManager.cosmetics);
        }
    }
}