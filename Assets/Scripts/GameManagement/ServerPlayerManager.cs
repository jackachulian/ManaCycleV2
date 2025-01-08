using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the spawning, despawning and tracking of player objects and assigning IDs / retreiving by ID.
/// Should only be used on the server!
/// </summary>
public class ServerPlayerManager : MonoBehaviour {
    /// <summary>
    /// Called when a player connects, after the player object is spawned. First arg is the connecting player's player object.
    /// </summary>
    public event Action<Player> OnPlayerSpawned;

    /// <summary>
    /// Called when a player disconnects, before the player object is despawned. First arg is the disconnecting player's player object.
    /// </summary>
    public event Action<Player> OnPlayerDespawning;


    const int playerLimit = 4;


    /// <summary>
    /// All players that are connected to the battle
    /// Index - int representing the board index of the player from 0-3
    /// Value - the player object, or null if no player with this ID
    /// ID assignment differs based on the implementation
    /// </summary>
    private Dictionary<ulong, Player> players = new Dictionary<ulong, Player>(4);

    /// <summary>
    /// Prefab used to instantiate player objects for connecting players
    /// </summary>
    [SerializeField] protected Player playerPrefab;

    /// <summary>
    /// Create a new player with the assigned ID.
    /// To be called by implementations of this class when a player connects to this playermanager.
    /// </summary>
    /// <param name="playerId">ID to assign to the new player</param>
    /// <returns>the newly created player object</returns?
    protected Player ServerAddPlayer(ulong playerId) {
        // Server/host only
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server can add new players!");
        }

        // Raise an error if lobby is already at capacity
        if (players.Count >= playerLimit) {
            Debug.LogError("There is not enough slots for the new player to connect! Max 4 players.");
            return null;
        }

        // Instantiate the player on the server and register in the players dictionary
        Player addedPlayer = Instantiate(playerPrefab);

        // Spawn the new player on the network (should persist between scenes!)
        addedPlayer.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);

        // Assign its ID network variable to be sync'd between clients
        addedPlayer.playerId.Value = playerId;

        Debug.Log("Added player with ID "+playerId);
        return addedPlayer;
    }

    /// <summary>
    /// Called by a Player's OnNetworkSpawn when it connects on a client.
    /// Adds the spawned player to the local dictionary of connected players.
    /// Not needed on the server because the player is already registered in the dictionary in ServerAddPlayer.
    /// </summary>
    public void NonServerAddPlayer(Player player) {
        if (NetworkManager.Singleton.IsServer) {
            Debug.LogError("Server is trying to call NonServerAddPlayer! Only non-servers should call this method.");
            return;
        }

        players[player.playerId.Value] = player;
    }

    /// <summary>
    /// Remove the player object with the assigned ID.
    /// <param name="playerId">ID of the disconnecting player</param>
    /// </summary>
    protected void ServerRemovePlayer(ulong playerId) {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server can remove players!");
            return;
        }

        if (!players.ContainsKey(playerId)) {
            Debug.LogWarning("Trying to remove player of id "+playerId+" but there is no connected player with that ID");
            return;
        }

        Player player = players[playerId];
        player.GetComponent<NetworkObject>().Despawn(destroy: true);

        players.Remove(playerId);
        Debug.Log("Removed player with ID "+playerId);
    }
}