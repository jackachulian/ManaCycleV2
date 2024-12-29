using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles player objects from the NetworkManager
/// </summary>
public class PlayerManager : MonoBehaviour {
    /// <summary>
    /// Contains shared values used between battle scenes
    /// </summary>
    [SerializeField] public BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// Cached networkmanager component on same gameobject
    /// </summary>
    private NetworkManager networkManager;

    /// <summary>
    /// All connected players.
    /// In online mode, key is the client ID
    /// In local mode, key is the device ID
    /// </summary>
    private Dictionary<ulong, BattlePlayer> players;


    private void Awake() {
        networkManager = GetComponent<NetworkManager>();

        if (battleLobbyManager.playerManager != null) {
            if (battleLobbyManager.playerManager == this) {
                Debug.LogWarning("Same BattleNetworkManager woke up twice?");
            } else {
                Debug.LogWarning("Duplicate BattleNetworkManager! Destroying the new one.");
                Destroy(gameObject);
                return;
            }
        }

        Debug.Log("PlayerManager initializing");
        battleLobbyManager.playerManager = this;
        battleLobbyManager.networkManager = networkManager;
        DontDestroyOnLoad(this);

        players = new Dictionary<ulong, BattlePlayer>();

        networkManager.OnClientStarted += OnStarted;
    }

    public void Start() {
        // If battle scene is already active, make sure host is started
        // (only really used for testing straight into battle scene and skipping battle setup)
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            Debug.Log("Starting host directly from network manager because we are in the battle scene");
            battleLobbyManager.StartNetworkManagerHost();
        }
    }

    public void OnStarted() {
        // sanity check :(
        if (networkManager.IsServer) {
            Debug.Log("I'm a server!");
        }
        if (networkManager.IsHost) {
            Debug.Log("I'm a host!");
        }
        if (networkManager.IsClient) {
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
    /// <param name="id">client ID  (online multiplayer) or device ID (local multiplayer/singleplayer) of the connected player</param>
    public void AddPlayer(ulong id, BattlePlayer player) {
        players[id] = player;
    }

    /// <summary>
    /// Called when a player object despawns.
    /// Removes them from the character dictionary.
    /// </summary>
    /// <param name="id">client ID / device ID of the disconnected player</param>
    public void RemovePlayer(ulong id) {
        players.Remove(id);
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
    /// Return the connected player object with the specified client/device ID
    /// </summary>
    public BattlePlayer GetPlayerById(ulong id) {
        if (players.ContainsKey(id)) {
            return players[id];
        } else {
            Debug.Log("Player with id "+id+" does not exist or has despawned / disconnected");
            return null;
        }
    }
}