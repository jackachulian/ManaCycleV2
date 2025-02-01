
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Defines all the values that will be used to determine battle properties, such as the seed, etc.
/// In online, this object is serialized and sent over the network before a match starts.
/// </summary>
[System.Serializable]
public struct BattleData : INetworkSerializeByMemcpy {
    /// <summary>
    /// Seed used for the piece color RNG. By default, this is random.
    /// </summary>
    public int seed;

    /// <summary>
    /// If true, the seed of the battle should be randomized before playing. Default is true, but is false for some levels.
    /// </summary>
    public bool randomizeSeed;

    // (These will later depend on the level in solo mode, but constant for now)
    public int cycleLength;
    public int cycleUniqueColors;

    /// <summary>
    /// Time limit, in seconds. If 0 or negative, time is unlimited.
    /// </summary>
    [Tooltip("Time limit, in seconds. If 0 or negative, time is unlimited.")]
    public int timeLimit;

    public BattleData(int cycleLength = 5, int cycleUniqueColors = 5) {
        this.seed = -1;
        this.randomizeSeed = true;
        this.cycleLength = cycleLength;
        this.cycleUniqueColors = cycleUniqueColors;
        this.timeLimit = 0;
    }

    public void SetDefaults() {
        cycleLength = 5;
        cycleUniqueColors = 5;
    }

    /// <summary>
    /// Randomize the piece RNG seed and the cycle color sequence. 
    /// Should only be called on the server/host and then that seed is sent to other clients.
    /// </summary>
    public void Randomize() {
        seed = Random.Range(0, int.MaxValue);
    }
}