using UnityEngine;
using TMPro;

/// <summary>
/// Manages the UI of the HP Bar.
/// Does NOT manage receiving damage, that is managed by the Board. This is a UI only behaviour.
/// </summary>
public class HealthUI : MonoBehaviour {
    [SerializeField] private GameObject hpBarObject;
    [SerializeField] private GameObject hpNumberObject;
    [SerializeField] private TMP_Text healthNumberLabel;
    [SerializeField] private Renderer hpBarRenderer;

    private Material hpBarMaterial;

    private void Awake() {
        hpBarMaterial = hpBarRenderer.material;
    }

    public void ShowHealthUI() {
        hpBarObject.SetActive(true);
        hpNumberObject.SetActive(true);
    }

    public void HideHealthUI() {
        hpBarObject.SetActive(false);
        hpNumberObject.SetActive(false);
    }

    public void UpdateUI(float hp, float maxHp, int[] incomingDamage) {
        if (healthNumberLabel) healthNumberLabel.text = hp + "";


        float hpPercentage = hp / maxHp;

        hpBarMaterial.SetFloat("_HpPercentage", hpPercentage);

        float [] incomingDamagePercentages = new float[6];
        for (int i=0; i<6; i++) {
            incomingDamagePercentages[i] = incomingDamage[i]/maxHp;
        }
        hpBarMaterial.SetFloatArray("_IncomingDamage", incomingDamagePercentages);
    }
}