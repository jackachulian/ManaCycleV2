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
    /// Used to start battles
    /// </summary>
    [SerializeField] private BattleStartNetworkBehaviour battleStart;

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
            player.ready.OnValueChanged += battleStart.OnAnyPlayerReadyChanged;
        }
    }

    public void OnPlayerLeft(ulong id) {
        if (battleLobbyManager.networkManager.IsServer) {
            // stop listening for readiness when player disconnects
            BattlePlayer player = battleLobbyManager.playerManager.GetPlayerById(id);
            if (player) player.ready.OnValueChanged -= battleStart.OnAnyPlayerReadyChanged;
        }
    }
}