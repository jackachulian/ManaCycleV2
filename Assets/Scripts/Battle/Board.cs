using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A board that can be controlled by a player or an AI.
/// Manages the tile grid and movement/placement of pieces.
/// </summary>
public class Board : MonoBehaviour
{
    /// <summary>
    /// All ManaTiles that have been placed on the board
    /// (This does not include any currently falling pieces on this board!)
    /// </summary>
    [SerializeField] private ManaTile[,] manaTileGrid;

    /// <summary>
    /// How often the current piece should fall down one tile
    /// </summary>
    [SerializeField] private float movementFrequency = 0.8f;

    /// <summary>
    /// Transform that mana tiles should be parented under
    /// </summary>
    [SerializeField] private Transform manaTileTransform;

    /// <summary>
    /// The battleManager that is managing this board. Also contains dependencies needed for SpawnPiece, etc
    /// Is set upon battle initialization.
    /// </summary>
    private BattleManager battleManager;

    /// <summary>
    /// Incrementing timer. When greater than movementFrequency (after fall speed modifiers), 
    /// set to 0 and move the current piece down one tile.
    /// </summary>
    private float movementTimer;

    /// <summary>
    /// RNG instance used to determine the colors.
    /// Seed is set on battle initialization.
    /// </summary>
    private System.Random rng;

    /// <summary>
    /// Current falling piece on this board.
    /// </summary>
    private ManaPiece currentPiece;

    /// <summary>
    /// Dimensions of the board's tile grid. 
    /// This is slightly taller than the visual size, due to the fact that pieces can be moved above the top edge of the board.
    /// </summary>
    private const int width = 7, height = 20;

    /// <summary>
    /// The visual height of the board.
    /// </summary>
    private const int visualHeight = 15;

    /// <summary>
    /// Called by BattleManager when battle is initialized
    /// Any initialization should go here (no Start() method)
    /// </summary>
    /// <param name="battleManager">the battle manager for the battle this board is being initialized within</param>
    /// <param name="seed">the seed to use for RNG</param>
    public void InitializeBattle(BattleManager battleManager, int seed) {
        this.battleManager = battleManager;
        manaTileGrid = new ManaTile[width, height];

        rng = new System.Random(seed);

        // TODO: move this to after a timer
        SpawnNewPiece();
    }

    // Update is called once per frame
    void Update()
    {
        // If there is a current piece, have it fall after delay
        if (currentPiece) {
            movementTimer += Time.deltaTime;
            if (movementTimer > movementFrequency) {
                movementTimer -= movementFrequency;

                // TODO: add a little bit of buffer time before placing the piece to allow sliding
                bool moved = TryMovePiece(Vector2Int.down);
                if (!moved) {
                    PlacePiece(currentPiece);
                    SpawnNewPiece();
                }
            }
        }
    }

    void SpawnNewPiece() {
        currentPiece = battleManager.SpawnPiece();
        currentPiece.transform.SetParent(manaTileTransform);

        // spawn position will be the top row, middle column
        Vector2Int spawnPos = new Vector2Int(width / 2, height-1); 
        currentPiece.position = spawnPos;
        currentPiece.UpdateVisualPositions();

        for (int i = 0; i < currentPiece.tiles.Length; i++) {
            int color = rng.Next(5);
            currentPiece.tiles[i].SetColor(color);
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
        currentPiece.position += offset;

        if (!IsValidPlacement(currentPiece)) {
            currentPiece.position -= offset;
            return false;
        }

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
    bool TryRotatePiece(int rotations) {
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

        currentPiece.UpdateVisualPositions();
        return true;
    }

    /// <summary>
    /// Returns true if all tiles in the piece are within bounds and not obstructed by any tiles already placed.
    /// </summary>
    bool IsValidPlacement(ManaPiece piece) {
        for (int i = 0; i < piece.tiles.Length; i++) {
            Vector2Int piecePosition = piece.position + piece.GetTilePosition(i);

            // If any tile in the piece is out of bounds return false
            if (piecePosition.x < 0 || piecePosition.x >= width || piecePosition.y < 0 || piecePosition.y >= height) {
                return false;
            }

            // If any tile in the piece intersects another tile, return false
            // (If value is not null, there is a tile there)
            if (manaTileGrid[piecePosition.x, piecePosition.y]) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Move all tiles of the piece onto this board's grid of tiles and destroy the piece container afterwards.
    /// Uses the piece's <c>GetTilePosition()</c> to get the positions to place each tile.
    /// </summary>
    void PlacePiece(ManaPiece piece) {
        Vector2Int[] placePositions = new Vector2Int[piece.tiles.Length];

        // Convert the position space of all tiles from piece-relative to board-relative (apply position and rotation)
        // and reparent the mana tiles to this board
        for (int i = 0; i < piece.tiles.Length; i++) {
            ManaTile tile = piece.tiles[i];
            Vector2Int boardPosition = piece.position + piece.GetTilePosition(i);
            tile.position = boardPosition;
            placePositions[i] = boardPosition;
            manaTileGrid[boardPosition.x, boardPosition.y] = tile;
            tile.transform.SetParent(manaTileTransform, true);
        }

        // Destroy piece container that is no longer needed
        Destroy(piece);

        // Perform gravity on all tiles - lower Y position tiles should fall first so tiles above correctly fall on top of them
        placePositions.OrderBy(pos => pos.y);
        foreach (Vector2Int pos in placePositions) {
            TileGravity(pos);
        }
    }

    /// <summary>
    /// Perform gravity on the tile at the given position.
    /// </summary>
    /// <param name="position">the position of the tile on the grid to fall</param>
    void TileGravity(Vector2Int position) {
        // if null, no tile here
        ManaTile tile = manaTileGrid[position.x, position.y];
        if (tile == null) return;

        // While the tile is above the bottom of the board, keep falling
        while (position.y > 0) {
            // If the tile below is not empty, tile cannot fall here
            if (manaTileGrid[position.x, position.y - 1] != null) {
                break;
            }

            // Fall by one tile
            manaTileGrid[position.x, position.y] = null;
            position.y -= 1;
            manaTileGrid[position.x, position.y] = tile;
        }

        // if tile fell at all, animate its fall position to the new position
        if (position.y != tile.position.y) {
            tile.AnimatePosition(position);
        }
    }
}
