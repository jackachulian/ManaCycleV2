using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the piece falling on this board.
/// Responsibilities include piece movement, spawning new randomized pieces, falling, etc.
/// </summary>
public class GhostPieceManager : MonoBehaviour {
    /// <summary>
    /// The Board this is managing the spellcasts of. Cached on InitializeBattle()
    /// </summary>
    private Board board;

    // ======== Blob detection ========
    /// <summary>
    /// A collection of tiles that show where tiles in the board's current piece will land.
    /// Should always have the same length as the amount of tiles in the current piece.
    /// </summary>
    private ManaTile[] ghostTiles;

    /// <summary>
    /// Simulated tile grid that copies the current state of the board, drops simulated ghost tiles on it, and checks if blobs are connected.
    /// All the values in this array besides the ghost tiles are references to tiles on the actual board, so both real and ghost tiles' glow can be adjusted.
    /// </summary>
    private ManaTile[,] simulatedTileGrid;

    /// <summary>
    /// Contains a grid of the simulated blob each tile has been added, or null for no blob. Used during connected tile searching.
    /// </summary>
    private List<Vector2Int>[,] simulatedBlobGrid;

    /// <summary>
    /// Initialize the battle.
    /// Is initialized after UpcomingPieceList so that the first piece can be spawned on initialization.
    /// (The piece is spawned before the timer starts, in case boards' countdowns start at slightly different times on different clients)
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;

        simulatedTileGrid = new ManaTile[board.manaTileGrid.width, board.manaTileGrid.height];
        simulatedBlobGrid = new List<Vector2Int>[board.manaTileGrid.width, board.manaTileGrid.height];
    }

    /// <summary>
    /// Should be called after a new current piece is spawned on the board.
    /// Create a ghost version of all the tiles on that piece.
    /// Uses the current BattleManager instance's cosmetics to determine what cosmetics should be used.
    /// </summary>
    public void CreateGhostPiece() {
        ManaPiece currentPiece = board.pieceManager.currentPiece;
        ghostTiles = new ManaTile[currentPiece.tiles.Length];

        for (int i = 0; i < ghostTiles.Length; i++) {
            ManaTile ghostTile = Instantiate(currentPiece.tiles[i]);
            ghostTiles[i] = ghostTile;
            ghostTile.SetColor(currentPiece.tiles[i].color, ghost: true, BattleManager.Instance.cosmetics);
            ghostTile.transform.SetParent(board.manaTileGrid.manaTileTransform, true);
        }
    }

    /// <summary>
    /// Should be called whenever the board's current piece changes columns or rotates. 
    /// Rebuilds the ghost blob.
    /// Only required when row of tiles of the piece change, because pieces will land in the same columns regardless of the current row.
    /// </summary>
    public void UpdateGhostPiece() {
        // Set the ghost tile grid to a fresh copy of the current state of the actual mana tile grid
        Array.Copy(board.manaTileGrid.tileGrid, simulatedTileGrid, board.manaTileGrid.width * board.manaTileGrid.height);

        ManaPiece piece = board.pieceManager.currentPiece;

        Vector2Int[] placePositions = new Vector2Int[piece.tiles.Length];

        // Convert the position space of all tiles from piece-relative to board-relative (apply position and rotation).
        // Create ghost tiles at the location of the piece on the board
        for (int i = 0; i < piece.tiles.Length; i++) {
            // We're just grabbing the position of the actual tile that exists on the board
            Vector2Int boardPosition = piece.position + piece.GetTilePosition(i);

            // Move the ghost tile, not the actual tile on the piece!
            ManaTile ghostTile = ghostTiles[i];
            ghostTile.SetPosition(boardPosition, false);
            placePositions[i] = boardPosition;
            simulatedTileGrid[boardPosition.x, boardPosition.y] = ghostTile;
        }

        // Perform simulated gravity on all ghost tiles
        Array.Sort(placePositions, (pos1, pos2) => pos1.y - pos2.y);
        foreach (Vector2Int pos in placePositions) {
            TileUtility.TileGravity(pos, ref simulatedTileGrid, false);
        }

        // Reset the blob array to empty
        for (int y = 0; y < board.manaTileGrid.height; y++) {
            for (int x = 0; x < board.manaTileGrid.width; x++) {
                simulatedBlobGrid[x,y] = null;
                // TODO: unglow a blob's tile if it is currently glowed. set glowing to false
            }
        }

        // Rebuild all blobs based on the state of the simulated tile grid
        // Only build blobs off tiles that are connected to a ghost tile; those are the only blobs that should be glowed
        for (int i = 0; i < piece.tiles.Length; i++) {
            ManaTile ghostTile = ghostTiles[i];

            // If this ghost tile has already been added to a blob (ghost tile blob connected to itself), skip this ghost tile
            Vector2Int position = new Vector2Int(ghostTile.position.x, ghostTile.position.y);
            List<Vector2Int> blob = simulatedBlobGrid[position.x, position.y];
            if (blob != null) continue;

            // Try building a blob off this ghost tile
            blob = new List<Vector2Int>();

            TileUtility.ExpandBlob(blob, position, ghostTile.color, board, ref simulatedTileGrid, ref simulatedBlobGrid);

            // if blob is clearable, glow all the connected mana
            if (blob.Count >= board.spellcastManager.minBlobSize) {
                // TODO: glow blobs here! set glowing to true
            }
        }
    }
}