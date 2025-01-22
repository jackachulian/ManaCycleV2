using System;
using UnityEngine;

public class SpellManager : MonoBehaviour {
    public event Action onSpChanged;



    [SerializeField] private SpellMenuUI spellMenuUI;

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

    public void SetSp(int sp) {
        this.sp = Mathf.Clamp(sp, 0, maxSp);
        onSpChanged?.Invoke();
    }

    public void AddSp(int sp) {
        SetSp(this.sp + sp);
    }

    public void RemoveSp(int sp) {
        SetSp(this.sp - sp);
    }

    public void OnPlayerAssigned()
    {
        if (!board.player.battler) return;

        if (board.player.battler.uniqueSpell)
        {
            board.player.battler.uniqueSpell.OnPlayerBoardConnected(board);
        }

        spellMenuUI.OnPlayerAssigned(board);    
    }

    public void UseSpell(Spell spell) {
        if (sp < spell.spCost) {
            Debug.LogWarning("Not enough SP to use "+spell.displayName);
            return;
        };
        RemoveSp(spell.spCost);
        spell.Use(board);
    }

    public void UseUniqueSpell() {
        if (!board || !board.player || !board.player.battler || !board.player.battler.uniqueSpell) return;
        UseSpell(board.player.battler.uniqueSpell);
    }

    public void UseSpell1() {
        if (!board || !board.player) return;
        UseSpell(board.player.spell1);
    }

    public void UseSpell2() {
        if (!board || !board.player) return;
        UseSpell(board.player.spell2);
    }
}