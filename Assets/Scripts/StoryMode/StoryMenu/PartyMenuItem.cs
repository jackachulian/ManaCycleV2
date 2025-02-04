using SaveDataSystem;
using StoryMode.Overworld;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMenuItem : MonoBehaviour {
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private TMP_Text nameLabel;

    private Battler battler;

    public void SetBattler(Battler battler) {
        this.battler = battler;
        battlerPortrait.sprite = battler.sprite;
        nameLabel.text = battler.displayName;
    }

    public void SelectBattler() {
        // set model
        OverworldManager.ChangeActiveBattler(battler);
    }
}