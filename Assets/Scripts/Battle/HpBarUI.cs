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

    public void UpdateHpVisual(float hp, float maxHp) {
        float hpPercentage = hp / maxHp;

        material.SetFloat("_HpPercentage", hpPercentage);
    }
}