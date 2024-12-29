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

    /// <summary>
    /// Matches a client/device ID with the index of the battlesetup panel / battle board they are currently assigned to.
    /// Client ID used in online multiplayer, and device ID used for other modes.
    /// </summary>
    private List<ulong> assignedIds;

    private void Awake() {
        networkManager = GetComponent<NetworkManager>();


        if (battleLobbyManager.playerManager != null) {
            if (battleLobbyManager.playerManager == this) {
                Debug.LogWarning("Same PlayerManager woke up twice?");
            } else {
                Debug.LogWarning("Duplicate PlayerManager! Destroying the new one.");
                Destroy(gameObject);
                return;
            }
        }

        Debug.Log("PlayerManager initializing");
        battleLobbyManager.playerManager = this;
        battleLobbyManager.networkManager = networkManager;
        DontDestroyOnLoad(this);

        players = new Dictionary<ulong, BattlePlayer>();
        assignedIds = new List<ulong>();

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
    /// Called when a player object spawns. Adds them to the player dictionary, and assign them a panel/board.
    /// </summary>
    /// <param name="id">client ID  (online multiplayer) or device ID (local multiplayer/singleplayer) of the connected player</param>
    public void AddPlayer(ulong id, BattlePlayer player) {
        players[id] = player;

        if (battleLobbyManager.networkManager.IsServer) {
            // Assign the new player to a panel/board
            AssignClientToNextAvailableBoardIndex(id);
        } else {
            Debug.Log("Not assigning player "+id+" to a panel because this is not the server");
        }
    }

    /// <summary>
    /// Called when a player object despawns.
    /// Removes them from the character dictionary.
    /// </summary>
    /// <param name="id">client ID / device ID of the disconnected player</param>
    public void RemovePlayer(ulong id) {
        if (battleLobbyManager.networkManager.IsServer) {
            UnassignClientFromPanel(id);
        }

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

    public void ConnectAllPlayersToBoards() {
        foreach (var player in GetPlayers()) {
            player.ConnectToBoard(player.boardIndex.Value);
        }
    }

    public void EnableBattleInputs() {
        foreach (var player in GetPlayers()) {
            player.EnableBattleInputs();
        }
    }

    public void DisableBattleInputs() {
        foreach (var player in GetPlayers()) {
            player.DisableBattleInputs();
        }
    }

    /// <summary>
    /// (Server/Host Only) Assigns a newly joining player to the next available panel.
    /// </summary>
    public void AssignClientToNextAvailableBoardIndex(ulong id) {
        // Show error when attempted from a non-server client.
        // Even if this error did no show up, clients would not be able to perform this because the server owns the CharacterSelectMenu network object.
        if (!battleLobbyManager.networkManager.IsServer) {
            Debug.LogError("Only the server can assign a client to a panel!");
            return;
        }

        if (assignedIds.Count >= 4) {
            Debug.LogError("There are no available panels to assign. Are there more than 4 players in the lobby?");
            return;
        }

        // Add the client/device ID to the list, and assign the ID to the tail of the list of panels (the next available one).
        assignedIds.Add(id);
        int boardIndex = assignedIds.Count - 1;
        BattlePlayer player = GetPlayerById(id);

        Debug.Log("Assigning board "+boardIndex+" to player with ID "+id);
        player.boardIndex.Value = boardIndex;
    }

    /// <summary>
    /// (Server/Host Only) Unassign the client/device ID from whatever panel they are assigned to.
    /// Afterwards, reassign all panels based on the assignedIds list.
    /// Therefore, if the client who disconnected has panels after them, those clients will shift backward and be reassigned to previous panels.
    /// </summary>
    public void UnassignClientFromPanel(ulong id) {
        if (!battleLobbyManager.networkManager.IsServer) {
            Debug.LogError("Only the server/host can unassign a client from a panel!");
            return;
        }

        bool clientWasRemoved = assignedIds.Remove(id);

        if (clientWasRemoved) {
            for (int i = 0; i < assignedIds.Count; i++) {
                ulong assignedId = assignedIds[i];
                BattlePlayer player = GetPlayerById(assignedId);
                if (player) {
                    player.boardIndex.Value = i;
                } else {
                    Debug.Log("Tried to unassign player "+assignedId+" but player object was null (probably despawned)");
                }
            }
        }
    }
}