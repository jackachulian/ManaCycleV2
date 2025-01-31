using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Level")]
public class Level : ScriptableObject {
    /// <summary>
    /// Used to identify this battle for save data, achievements, etc
    /// </summary>
    [Tooltip("Used to identify this battle for save data, achievements, etc")]
    public string id;

    /// <summary>
    /// display name (TODO: localize)
    /// </summary>
    public string displayName;


    [System.Serializable]
    public class LevelPlayer {
        /// <summary>
        /// Battler that this player can use. 
        /// If list has more than one battler, player can choose from that list. 
        /// If list is empty, player can choose any battler.
        /// </summary>
        [Tooltip("Battler that this player can use. \nIf list has more than one battler, player can choose from that list. \nIf list is empty, player can choose any battler.")]
        public Battler[] battler;

        /// <summary>
        /// If this battler's unique spell can be used in this battle.
        /// </summary>
        [Tooltip("If this battler's unique spell can be used in this battle.")]
        public bool uniqueSpellEnabled = true;

        /// <summary>
        /// If list is not empty, this player will have these spells auto-equipped and cannot be swapped out.
        /// </summary>
        [Tooltip("If list is not empty, this player will have these spells auto-equipped and cannot be swapped out.")]
        public Spell[] fixedSpells;

        /// <summary>
        /// If this is a CPU, how fast the CPU acts.
        /// </summary>
        [Tooltip("If this is a CPU, the amount of decisions it makes per second.")]
        public float cpuSpeed = 3;
    }

    /// <summary>
    /// Each player in this battle. Index 0 is the client's player, other indexes are CPU opponents that are added to the battle.
    /// </summary>
    [Tooltip("Each player in this battle. Index 0 is the client's player, other indexes are CPU opponents that are added to the battle.")]
    public LevelPlayer[] levelPlayer;

    /// <summary>
    /// The player has this many open spell slots to equip whatever spell they want. 
    /// </summary>
    [Tooltip("Client-controlled player has this many open slots to equip whatever spells they want. The last player-selected spells will be auto-filled in these slots at the start of battle setup.")]
    public int playerOpenSpellSlots = 2;


    /// <summary>
    /// If true, each time the level is played a different random RNG seed will be used.
    /// If false, the same seed in BattleData will be used every battle.
    /// </summary>
    [Tooltip("If this level should have different cycle and piece colors each time it is played.")]
    public bool randomizeSeedEachBattle = true;

    /// <summary>
    /// Contains battle-specific non-player-specific data used in the battle scene such as the seed selected, cycle lengths, etc.
    /// </summary>
    public BattleData battleData = new BattleData();
}