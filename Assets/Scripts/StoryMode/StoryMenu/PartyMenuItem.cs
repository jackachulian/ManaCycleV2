using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMenuItem : MonoBehaviour {
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private TMP_Text nameLabel;

    public void SetBattler(Battler battler) {
        battlerPortrait.sprite = battler.sprite;
        nameLabel.text = battler.displayName;
    }
}