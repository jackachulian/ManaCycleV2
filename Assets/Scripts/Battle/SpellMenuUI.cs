using TMPro;
using UnityEngine;

public class SpellMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text spNumberLabel;

    // Layout of spell items placed in the board prefab,
    // of which the values and sprites will be changed to match the spell assigned to it.
    [SerializeField] private SpellItem[] spellItems;

    /// <summary>
    /// board currently assigned to
    /// </summary>
    private Board board;


    [SerializeField] private Renderer spBarRenderer;

    private Material spBarMaterial;

    private void Awake() {
        spBarMaterial = spBarRenderer.material;
    }

    /// <summary>
    /// Called whenever board's current SP changes.
    /// </summary>
    public void UpdateUI() {
        if (spNumberLabel) spNumberLabel.text = board.spellManager.sp + "";

        float spPercentage = 1.0f*board.spellManager.sp / board.spellManager.maxSp;

        spBarMaterial.SetFloat("_HpPercentage", spPercentage);
    }

    /// <summary>
    /// Called by a board when a player is assigned to it.
    /// </summary>
    public void OnPlayerAssigned(Board board) {
        this.board = board;

        if (!board.player || !board.player.battler) return;

        spellItems[0].AssignSpell(board.player.battler.uniqueSpell);
        spellItems[1].AssignSpell(board.player.spell1);
        spellItems[2].AssignSpell(board.player.spell2);

        UpdateUI();
        board.spellManager.onSpChanged += UpdateUI;
    }
}
