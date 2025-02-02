using UnityEngine;

public class IronSwordSpell : Spell
{
    public ManaPiece piecePrefab;

    [SerializeField] private int damagePerTileCleared = 20;

    public override void Use(Board board)
    {
        var piece = Instantiate(piecePrefab);

        piece.onVisualPositionUpdated += OnVisualPositionUpdated;
        piece.onPlaced += OnPiecePlaced;

        board.pieceManager.ReplaceCurrentPiece(piece);
    }

    // When the tile's position is changed, pulse glow all tiles in the same column,
    // to show that they will be cleared if dropped in the current column.
    void OnVisualPositionUpdated(ManaPiece piece) {
        for (int y = 0; y < board.manaTileGrid.height; y++)
        {
            ManaTile tile = board.manaTileGrid.GetTile(new Vector2Int(piece.position.x, y));
            if (tile)
            {
                tile.SetPulseGlow(true);
                tile.UpdateVisuals(board: board);
            }
        }
    }

    void OnPiecePlaced(ManaPiece piece) {
        int tilesCleared = 0;

        // Clear the sword piece's center tile
        board.manaTileGrid.ClearTile(new Vector2Int(piece.position.x, piece.position.y));

        // Clear all tiles in the target column
        for (int y = 0; y < board.manaTileGrid.height; y++)
        {
            bool cleared = board.manaTileGrid.ClearTile(new Vector2Int(piece.position.x, y));
            if (cleared) tilesCleared++;
        }

        board.healthManager.DealDamage(tilesCleared * damagePerTileCleared);
    }
}