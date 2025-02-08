using SaveDataSystem;
using StoryMode.Overworld;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipMenuItem : MonoBehaviour {
    [SerializeField] private Image supportPortrait;
    [SerializeField] private TMP_Text nameLabel;

    private Spell spell;

    public void SetSpell(Spell spell) {
        this.spell = spell;
        supportPortrait.sprite = spell.sprite;
        nameLabel.text = spell.displayName;
    }

    public void EquipSpell() {
        // TODO: change the equipped spell1/spell2 in overworld manager which will also update save file
    }
}