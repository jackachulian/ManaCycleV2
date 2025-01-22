using UnityEngine;

public class IronSwordSpell : Spell
{
    public ManaPiece piecePrefab;

    public override void Use(Board board)
    {
        var piece = Instantiate(piecePrefab);

        piece.onVisualPositionUpdated += OnVisualPositionUpdated;

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
}