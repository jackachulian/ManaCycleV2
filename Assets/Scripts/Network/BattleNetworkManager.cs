using Unity.Netcode;
using UnityEngine;

public class BattleNetworkManager : NetworkManager {
    public static BattleNetworkManager instance {get; set;}

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("Overriding old battlenetworkmanager! Make sure it's only loaded once!");
        }

        instance = this;
    }

    public void ConnectPlayersToBoards() {
        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        Debug.Log("Players: "+players+" - count: "+players.Length);

        foreach (NetworkPlayer player in players) {
            player.BattleConnectBoard();
        }
    }

    public void ConnectPlayersToBattleSetup() {
        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);

        foreach (NetworkPlayer player in players) {
            player.BattleSetupConnectPanel();
        }
    }
}