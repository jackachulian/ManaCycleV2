using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Level")]
public class Level : ScriptableObject {
    public string displayName;

    public enum SpellType {
        NoSpells,
        EquippedSpells,
        FixedSpells
    }
    public SpellType spellType;

    /// <summary>
    /// Battler that each board must use in this battle. (The player is index 0)
    /// Null means any battler
    /// </summary>
    [Tooltip("Battler that each board must use in this battle. (The player is index 0). Null is any battler.")]
    public Battler[] fixedBattlers;

    /// <summary>
    /// Time limit, in seconds. If 0 or negative, time is unlimited.
    /// </summary>
    [Tooltip("Time limit, in seconds. If 0 or negative, time is unlimited.")]
    public int timeLimit;
}