using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Mana Cosmetics", menuName = "ManaCycle/ManaCosmetics", order = 1)]
public class ManaCosmetics : ScriptableObject
{
    /// <summary>
    /// Visuals for each color.
    /// 0=red, 1=green, 2=blue, 3=yellow, 4=purple
    /// </summary>
    public ManaVisual[] manaVisuals;

    /// <summary>
    /// Collection of mana visuals created during runtime in BattleManager's InitializeBattle. Used for showing what mana is connected to ghost tiles and will be cleared if placed in that position.
    /// </summary>
    public ManaVisual[] pulseGlowManaVisuals { get; set; }


    public void GeneratePulseGlowMaterials()
    {
        // Generate the pulse-glow materials
        pulseGlowManaVisuals = new ManaVisual[manaVisuals.Length];
        for (int i = 0; i < manaVisuals.Length; i++)
        {
            ManaVisual visual = manaVisuals[i];
            ManaVisual pulseGlowVisual = new ManaVisual();

            pulseGlowVisual.material = new Material(visual.material);
            pulseGlowVisual.material.SetFloat("_LitAmount", 0.25f);
            pulseGlowVisual.material.SetFloat("_PulseGlowAmplitude", 0.4f);
            pulseGlowVisual.material.SetFloat("_PulseGlowFrequency", 2.5f);

            pulseGlowVisual.ghostMaterial = new Material(visual.ghostMaterial);
            pulseGlowVisual.ghostMaterial.SetFloat("_LitAmount", 0.25f);
            pulseGlowVisual.ghostMaterial.SetFloat("_PulseGlowAmplitude", 0.4f);
            pulseGlowVisual.material.SetFloat("_PulseGlowFrequency", 2.5f);


            pulseGlowManaVisuals[i] = pulseGlowVisual;
        }
    }
}

[System.Serializable]
public class ManaVisual {
    /// <summary>
    /// Material to apply to the quad on ManaTiles of this type
    /// </summary>
    public Material material;

    /// <summary>
    /// Material to apply to the quad on ManaTiles of this type if the manaTile is a ghost tile
    /// </summary>
    public Material ghostMaterial;

}