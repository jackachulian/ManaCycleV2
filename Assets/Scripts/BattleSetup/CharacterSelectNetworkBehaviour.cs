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
            // listen for when their readiness changes to know when to check if all players are ready and start the game if so
            BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);
            player.ready.OnValueChanged += OnAnyPlayerReadyChanged;
        }
    }

    public void OnPlayerLeft(ulong id) {
        if (battleLobbyManager.networkManager.IsServer) {
            // stop listening for readiness when player disconnects
            BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);
            if (player) player.ready.OnValueChanged -= OnAnyPlayerReadyChanged;
        }
    }

    public void OnAnyPlayerReadyChanged(bool previous, bool current) {
        StartIfAllPlayersReady();
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
            battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE;
            battleLobbyManager.networkManager.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
        }
    }
}