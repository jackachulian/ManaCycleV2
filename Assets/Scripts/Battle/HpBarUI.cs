using UnityEngine;

/// <summary>
/// Manages the UI of the HP Bar.
/// Does NOT manage receiving damage, that is managed by the Board. This is a UI only behaviour.
/// </summary>
public class HpBarUI : MonoBehaviour {
    private Material material;

    private void Awake() {
        material = GetComponent<SpriteRenderer>().material;
    }

    public void UpdateUI(float hp, float maxHp, int[] incomingDamage) {
        float hpPercentage = hp / maxHp;

        material.SetFloat("_HpPercentage", hpPercentage);

        float [] incomingDamagePercentages = new float[6];
        for (int i=0; i<6; i++) {
            incomingDamagePercentages[i] = incomingDamage[i]/maxHp;
        }
        material.SetFloatArray("_IncomingDamage", incomingDamagePercentages);
    }
}