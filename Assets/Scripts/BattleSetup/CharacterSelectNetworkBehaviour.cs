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

    [SerializeField] private CharacterSelectMenu characterSelectMenu;

    /// <summary>
    /// Used in online multiplayer mode. Matches a client ID with the index of the player panel they are currently assigned to.
    /// </summary>
    private List<ulong> assignedClientIds = new List<ulong>();

    public override void OnNetworkSpawn() {
        battleLobbyManager.battleNetworkManager.OnClientConnectedCallback += OnPlayerJoined;
        battleLobbyManager.battleNetworkManager.OnClientDisconnectCallback += OnPlayerLeft;

        Debug.Log("character select network behaviour spawned!");
    }

    public void OnPlayerJoined(ulong clientId) {
        
        if (IsServer) {
            BattlePlayer player = battleLobbyManager.battleNetworkManager.GetPlayerByClientId(clientId);
            // If this is the server, listen for when their readiness changes to know when to check if all players are ready and start the game if so
            player.ready.OnValueChanged += OnAnyPlayerReadyChanged;
        }
    }

    public void OnPlayerLeft(ulong clientId) {
        if (IsServer) {
            UnassignClientFromPanel(clientId);

            BattlePlayer player = battleLobbyManager.battleNetworkManager.GetPlayerByClientId(clientId);
            if (player) player.ready.OnValueChanged -= OnAnyPlayerReadyChanged;
        }
    }

    public void OnAnyPlayerReadyChanged(bool previous, bool current) {
        StartIfAllPlayersReady();
    }

    /// <summary>
    /// (Server/Host Only) Assigns a newly joining player to the next available panel.
    /// </summary>
    /// <param name="clientId"></param>
    public void AssignClientToNextAvailablePanel(ulong clientId) {
        // Show error when attempted from a non-server client.
        // Even if this error did no show up, clients would not be able to perform this because the server owns the CharacterSelectMenu network object.
        if (!battleLobbyManager.battleNetworkManager.IsServer) {
            Debug.LogError("Only the server can assign a client to a panel!");
            return;
        }

        if (assignedClientIds.Count >= 4) {
            Debug.LogError("There are no available panels to assign. Are there more than 4 players in the lobby?");
            return;
        }

        // Add the client ID to the list, and assign the client ID to the tail of the list of panels (the next available one).
        assignedClientIds.Add(clientId);
        int boardIndex = assignedClientIds.Count - 1;
        Debug.Log("Assigning panel "+boardIndex+" to client "+clientId);

        BattlePlayer player = battleLobbyManager.battleNetworkManager.GetPlayerByClientId(clientId);
        player.boardIndex.Value = boardIndex;
    }

    /// <summary>
    /// (Server/Host Only) Unassign the client ID from whatever panel they are assigned to.
    /// Afterwards, reassign all panels based on the assignedClientIds list.
    /// Therefore, if the client who disconnected has panels after them, those clients will shift backward and be reassigned to previous panels.
    /// </summary>
    public void UnassignClientFromPanel(ulong clientId) {
        if (!battleLobbyManager.battleNetworkManager.IsServer) {
            Debug.LogError("Only the server/host can unassign a client from a panel!");
            return;
        }

        bool clientWasRemoved = assignedClientIds.Remove(clientId);

        if (clientWasRemoved) {
            for (int i = 0; i < assignedClientIds.Count; i++) {
                ulong assignedClientId = assignedClientIds[i];
                BattlePlayer player = battleLobbyManager.battleNetworkManager.GetPlayerByClientId(assignedClientId);
                player.boardIndex.Value = i;
            }
        }
    }

    /// <summary>
    /// (Server/Host Only) Check if all connected players are ready, and start the game if so.
    /// </summary>
    public void StartIfAllPlayersReady() {
        if (!battleLobbyManager.battleNetworkManager.IsServer) {
            Debug.LogError("Only the server/host can start the game!");
            return;
        }

        // Make sure there are at least 2 players before the match can start
        if (battleLobbyManager.battleNetworkManager.ConnectedClients.Count < 2) return;

        // Make sure all connected players are ready
        foreach (var player in battleLobbyManager.battleNetworkManager.GetPlayers()) {
            if (!player.ready.Value) {
                return;
            }
        }

        // The server/host chooses the seed that will be used for piece RNG.
        // TODO: may want to initialize these settings at the start of the scene, use them in the UI, 
        // and then send it to startgamerpc from here when the game starts
        BattleSettings settings = new BattleSettings();
        settings.seed = Random.Range(0, int.MaxValue);

        // send the RPC that will start the game on all clients
        StartGameRpc(settings);
    }

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc(BattleSettings settings) {
        BattleManager.Configure(settings);
        SceneManager.LoadScene("Battle");
    }
}