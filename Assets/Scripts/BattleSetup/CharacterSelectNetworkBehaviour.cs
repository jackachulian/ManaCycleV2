using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Character select functionality that should only be run on the server or hosting client.
/// This is a server-owned NetworkObject.
/// </summary>
public class CharacterSelectNetworkBehaviour : NetworkBehaviour {
    [SerializeField] private BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// Matches a client/device ID with the index of the player panel they are currently assigned to.
    /// Client ID used in online multiplayer, and device ID used for other modes.
    /// </summary>
    private List<ulong> assignedIds = new List<ulong>();

    public override void OnNetworkSpawn() {
        // In online multiplayer, listen for connected clients
        // if (battleLobbyManager.battleType == BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER) {
        //     battleLobbyManager.networkManager.OnClientConnectedCallback += OnPlayerJoined;
        //     battleLobbyManager.networkManager.OnClientDisconnectCallback += OnPlayerLeft;
        // } 
        // otherwise, player input manager will send OnPlayerJoined to this script

        Debug.Log("character select network behaviour spawned!");
    }

    public void OnPlayerJoined(ulong id) {
        Debug.Log("Player with id "+id+" joined");
        if (battleLobbyManager.networkManager.IsServer) {
            BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);

            // Assign the new player to a panel
            AssignClientToNextAvailablePanel(id);

            // listen for when their readiness changes to know when to check if all players are ready and start the game if so
            player.ready.OnValueChanged += OnAnyPlayerReadyChanged;
        } else {
            Debug.Log("Not assigning player "+id+" to a panel because this is not the server");
        }
    }

    public void OnPlayerLeft(ulong id) {
        if (IsServer) {
            UnassignClientFromPanel(id);

            BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);
            if (player) player.ready.OnValueChanged -= OnAnyPlayerReadyChanged;
        }
    }

    public void OnAnyPlayerReadyChanged(bool previous, bool current) {
        StartIfAllPlayersReady();
    }

    /// <summary>
    /// (Server/Host Only) Assigns a newly joining player to the next available panel.
    /// </summary>
    public void AssignClientToNextAvailablePanel(ulong id) {
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
        Debug.Log("Assigning panel "+boardIndex+" to player with ID "+id);

        BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);
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
                BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(assignedId);
                if (player) {
                    player.boardIndex.Value = i;
                } else {
                    Debug.Log("Tried to unassign player "+assignedId+" but player object was null (probably despawned)");
                }
            }
        }
    }

    /// <summary>
    /// (Server/Host Only) Check if all connected players are ready, and start the game if so.
    /// </summary>
    public void StartIfAllPlayersReady() {
        if (!battleLobbyManager.networkManager.IsServer) {
            Debug.LogError("Only the server/host can start the game!");
            return;
        }

        // in Online, Make sure there are at least 2 players before the match can start
        // in local, there only needs to be at least 1 player
        int minPlayers = battleLobbyManager.battleType == BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER ? 2 : 1;
        if (battleLobbyManager.networkManager.ConnectedClients.Count < minPlayers) return;

        // Make sure all connected players are ready
        foreach (var player in battleLobbyManager.playerManager.GetPlayers()) {
            if (!player.ready.Value) {
                return;
            }
        }

        // The server/host chooses the seed that will be used for piece RNG.
        // TODO: may want to initialize these settings at the start of the scene, use them in the UI, 
        // and then send it to startgamerpc from here when the game starts
        BattleData battleData = new BattleData();

        // TODO: maybe customizable in a battle settings UI?
        battleData.cycleLength = 7;
        battleData.cycleUniqueColors = 5;

        // will randomize the piece RNG seed and the cycle
        battleData.Randomize();

        // send the RPC that will start the game on all clients with synchronized battle data
        StartGameRpc(battleData);
    }

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc(BattleData battleData) {
        battleLobbyManager.SetBattleData(battleData);
        
        if (battleLobbyManager.networkManager.IsServer) {
            battleLobbyManager.networkManager.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
        }
    }
}