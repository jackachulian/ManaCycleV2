using UnityEngine;

public class EquipMenu : SimpleShowableMenu {
    [SerializeField] private EquipMenuItem equipMenuItemPrefab;
    [SerializeField] private Transform equipMenuLayoutTransform;

    protected override void OnEnable() {
        base.OnEnable();
        foreach (Transform child in equipMenuLayoutTransform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < OverworldManager.Instance.equippableSpells.Length; i++) {
            Spell spell = OverworldManager.Instance.equippableSpells[i];
            EquipMenuItem equipMenuItem = Instantiate(equipMenuItemPrefab, equipMenuLayoutTransform);
            equipMenuItem.SetSpell(spell);
            if (i == 0) SetFirstObjectSelected(equipMenuItem.gameObject);
        }
    }
}