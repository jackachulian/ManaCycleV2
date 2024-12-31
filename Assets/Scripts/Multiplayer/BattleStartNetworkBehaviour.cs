using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Necessary code to start a new battle.
/// Reused on both the CharacterSelect and BattleManager network objects.
/// (This behaviour is only used on the server, should be unused on clients)
/// </summary>
public class BattleStartNetworkBehaviour : NetworkBehaviour {
    [SerializeField] private BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// If this can start the battle.
    /// On charselect this is automatically true
    /// On battlemanager this is false but becomes true when a player wins the game
    /// </summary>
    public bool canStartBattle;

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

        if (!canStartBattle) {
            Debug.LogError("A battle cannot be started right now!");
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

        // Set ready of the local player back to false so their next ready choice will be for next rematch choice / characterselect choice.
        battleLobbyManager.playerManager.GetLocalPlayer().ready.Value = false;
        
        if (battleLobbyManager.networkManager.IsServer) {
            battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE;
            battleLobbyManager.networkManager.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
        }
    }
}