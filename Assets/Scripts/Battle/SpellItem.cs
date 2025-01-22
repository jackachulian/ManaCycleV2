using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellItem : MonoBehaviour {
    [SerializeField] private TMP_Text costLabel;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Image image;

    /// <summary>
    /// Update the UI of this spell item to match the spell, or hide this item if the passed spell is null.
    /// </summary>
    public void AssignSpell(Spell spell) {
        if (spell == null) {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (spell.sprite) {
            image.enabled = true;
            image.sprite = spell.sprite;
        } else {
            image.enabled = false;
        }
        costLabel.text = spell.spCost+"";
        nameLabel.text = spell.displayName;
    }
}