
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
    public int seed {get; private set;}

    // (These will later depend on the level in solo mode, but constant for now)
    public int cycleLength {get; private set;}
    public int cycleUniqueColors {get; private set;}

    public BattleData(int cycleLength = 5, int cycleUniqueColors = 5) {
        this.seed = -1;
        this.cycleLength = cycleLength;
        this.cycleUniqueColors = cycleUniqueColors;
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