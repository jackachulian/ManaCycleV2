using UnityEngine;

[CreateAssetMenu(fileName = "Mana Cosmetics", menuName = "ManaCycle/ManaCosmetics", order = 1)]
public class ManaCosmetics : ScriptableObject
{
    /// <summary>
    /// Visuals for each color.
    /// 0=red, 1=green, 2=blue, 3=yellow, 4=purple
    /// </summary>
    public ManaVisual[] manaVisuals;
}

[System.Serializable]
public class ManaVisual {
    public Sprite image;
    public Color tint;
}