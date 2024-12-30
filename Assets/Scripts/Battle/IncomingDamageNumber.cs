using TMPro;
using UnityEngine;

public class IncomingDamageNumber : MonoBehaviour {
    [SerializeField] private TMP_Text damageText;

    public void SetDamage(int damage) {
        if (damage > 0) {
            damageText.text = damage+"";
        } else {
            damageText.text = "";
        }
    }
}