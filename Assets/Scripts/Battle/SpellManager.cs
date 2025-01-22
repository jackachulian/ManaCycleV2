using UnityEngine;

public class SpellManager : MonoBehaviour {
    /// <summary>
    /// current SP. Used to use spells.
    /// </summary>
    public int sp {get; private set;} = 100;

    /// <summary>
    /// Maximum SP. May or may not depend on the character.
    /// </summary>
    public int maxSp {get; private set;} = 100;


    private Board board;

    public void InitializeBattle(Board board) {
        this.board = board;
    }

    public void OnPlayerAssigned()
    {
        if (!board.player.battler) return;

        if (board.player.battler.uniqueSpell)
        {
            board.player.battler.uniqueSpell.OnPlayerBoardConnected(board);
        }
    }

    public void UseUniqueSpell() {
        if (!board || !board.player || !board.player.battler || !board.player.battler.uniqueSpell) return;
        UseSpell(board.player.battler.uniqueSpell);
    }

    public void UseSpell(Spell spell) {
        if (sp < spell.spCost) return;
        spell.Use(board);
    }
}