using SaveDataSystem;
using UnityEngine;
using UnityEngine.UI;

public class StoryMenu : SimpleShowableMenu {
    [SerializeField] private MenuPanelSwapper _menuPanelSwapper;
    public MenuPanelSwapper menuPanelSwapper => _menuPanelSwapper;

    [SerializeField] private Image activeBattlerPortrait;

    protected override void OnEnable() {
        base.OnEnable();
        OverworldManager.onActiveBattlerChanged += SetActiveBattlerSprite;
    }

    protected override void OnDisable() {
        base.OnEnable();
        OverworldManager.onActiveBattlerChanged += SetActiveBattlerSprite;
    }

    public void SetActiveBattlerSprite(Battler battler) {
        activeBattlerPortrait.sprite = battler.sprite;
    }
}