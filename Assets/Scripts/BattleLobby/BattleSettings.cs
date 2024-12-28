
using Unity.Netcode;

/// <summary>
/// Defines all the values that will be used to determine battle properties, such as the seed, etc.
/// In online, this object is serialized and sent over the network before a match starts.
/// </summary>
public struct BattleSettings : INetworkSerializeByMemcpy {
    /// <summary>
    /// Seed used for the piece color RNG. By default, this is random.
    /// </summary>
    public int seed;
}