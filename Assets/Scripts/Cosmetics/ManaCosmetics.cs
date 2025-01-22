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
    /// Visual to use for chrome-colored mana, aka white, or counts as all colors. (color = -1)
    /// The ghost tile is used for most ability tiles to show the drop position.
    /// </summary>
    public ManaVisual chromeManaVisual;
}

[System.Serializable]
public class ManaVisual {
    /// <summary>
    /// Material to apply to the sprite on ManaTiles of this type
    /// </summary>
    public Material material;

    /// <summary>
    /// Material to apply to the sprite on ManaTiles of this type if the manaTile is a ghost tile
    /// </summary>
    public Material ghostMaterial;

}