
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Defines all the values that will be used to determine battle properties, such as the seed, etc.
/// In online, this object is serialized and sent over the network before a match starts.
/// </summary>
public struct BattleData : INetworkSerializeByMemcpy {
    /// <summary>
    /// Seed used for the piece color RNG. By default, this is random.
    /// </summary>
    public int seed;

    // (These will later depend on the level in solo mode, but constant for now)
    public int cycleLength;
    public int cycleUniqueColors;

    public BattleData(int cycleLength, int cycleUniqueColors) {
        this.seed = -1;
        this.cycleLength = cycleLength;
        this.cycleUniqueColors = cycleUniqueColors;
    }

    /// <summary>
    /// Randomize the piece RNG seed and the cycle color sequence. 
    /// Should only be called on the server/host and then that seed is sent to other clients.
    /// </summary>
    public void Randomize() {
        seed = Random.Range(0, int.MaxValue);
    }
}