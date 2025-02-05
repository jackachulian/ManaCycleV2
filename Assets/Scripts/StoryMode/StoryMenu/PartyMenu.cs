using UnityEngine;

public class PartyMenu : SimpleShowableMenu {
    [SerializeField] private PartyMenuItem partyMenuItemPrefab;
    [SerializeField] private Transform partyMenuLayoutTransform;

    protected override void OnEnable() {
        base.OnEnable();
        foreach (Transform child in partyMenuLayoutTransform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < OverworldManager.Instance.playableBattlers.Length; i++) {
            Battler battler = OverworldManager.Instance.playableBattlers[i];
            PartyMenuItem partyMenuItem = Instantiate(partyMenuItemPrefab, partyMenuLayoutTransform);
            partyMenuItem.SetBattler(battler);
            if (i == 0) SetFirstObjectSelected(partyMenuItem.gameObject);
        }
    }
}