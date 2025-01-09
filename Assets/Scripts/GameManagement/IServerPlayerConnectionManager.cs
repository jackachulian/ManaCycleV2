using System;
using Unity.Netcode;

/// <summary>
/// Abstraction for handling the connection and disconnection of players 
/// in either online or local multiplayer based on the conneciton method.
/// Should only be used on the server/host in online mode.
/// </summary>
public interface IServerPlayerConnectionManager {
    /// <summary>
    /// Called when the game and PlayerManager are initialized by the GameManager, and after the NetworkManager singleton is started.
    /// </summary>
    public void StartListeningForPlayers();

    /// <summary>
    /// Called when the game is no longer in the character select phase, or when the game is stopped.
    /// </summary>
    public void StopListeningForPlayers();

    public void OnPlayerSpawned(Player player);
    public void OnPlayerDespawned(Player player);
}