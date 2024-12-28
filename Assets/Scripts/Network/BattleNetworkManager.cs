using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleNetworkManager : NetworkManager {
    private BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// All connected players and their client IDs
    /// </summary>
    private Dictionary<ulong, BattlePlayer> players;

    /// <summary>
    /// Initialize via the BattleLobbyManager.
    /// </summary>
    public void Initialize(BattleLobbyManager battleLobbyManager) {
        this.battleLobbyManager = battleLobbyManager;

        players = new Dictionary<ulong, BattlePlayer>();

        OnClientStarted += OnStarted;

        // If battle scene is already active, make sure host is started
        // (only really used for testing straight into battle scene and skipping battle setup)
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            Debug.Log("Starting host directly from network manager because we are in the battle scene");
            battleLobbyManager.StartNetworkManagerHost();
        }
    }

    public void OnStarted() {
        // sanity check :(
        if (IsServer) {
            Debug.Log("I'm a server!");
        }
        if (IsHost) {
            Debug.Log("I'm a host!");
        }
        if (IsClient) {
            Debug.Log("I'm a client!");
        }

        // If this is the host, spawn the character select network object
        // if (IsHost) {
        //     battleLobbyManager.battleSetupManager.characterSelectMenu.SpawnNetworkObject();
        // }
    }

    /// <summary>
    /// Called when a player object spawns. Adds them to the player dictionary.
    /// </summary>
    /// <param name="clientId">client ID of the connected player</param>
    public void AddPlayer(ulong clientId, BattlePlayer player) {
        players[clientId] = player;
    }

    /// <summary>
    /// Called when a player object despawns.
    /// Removes them from the character dictionary.
    /// </summary>
    /// <param name="clientId">client ID of the connected player</param>
    public void RemovePlayer(ulong clientId) {
        players.Remove(clientId);
    }

    /// <summary>
    /// Get the total amount of connected player objects
    /// </summary>
    public int GetPlayerCount() {
        return players.Count;
    }

    /// <summary>
    /// Return a collection of all connected player objects
    /// </summary>
    public Dictionary<ulong, BattlePlayer>.ValueCollection GetPlayers() {
        return players.Values;
    }

    /// <summary>
    /// Return the connected player object with the specified client ID
    /// </summary>
    public BattlePlayer GetPlayerByClientId(ulong clientId) {
        if (players.ContainsKey(clientId)) {
            return players[clientId];
        } else {
            return null;
        }
    }
}