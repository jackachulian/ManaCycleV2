using System;
using Unity.Netcode;

/// <summary>
/// Abstraction for handling the connection and disconnection of players 
/// in either online or local multiplayer based on the conneciton method.
/// Should only be used on the server/host in online mode.
/// </summary>
public interface IServerPlayerConnectionManager {
    /// <summary>
    /// Called when a player connects, after the player object is spawned. First arg is the connecting player's requested ID.
    /// </summary>
    public event Action<ulong> onPlayerConnected;

    /// <summary>
    /// Called when a player disconnects, before the player object is despawned. First arg is the disconnecting player's ID.
    /// </summary>
    public event Action<ulong> onPlayerDisconnected;

    /// <summary>
    /// Called when the game and PlayerManager are initialized by the GameManager, and after the NetworkManager singleton is started.
    /// </summary>
    public void StartListeningForPlayers();

    /// <summary>
    /// Called when the game is no longer in the character select phase, or when the game is stopped.
    /// </summary>
    public void StopListeningForPlayers();
}