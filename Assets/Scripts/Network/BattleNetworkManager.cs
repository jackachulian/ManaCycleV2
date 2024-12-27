using Unity.Netcode;
using UnityEngine;

public class BattleNetworkManager : NetworkManager {
    public static BattleNetworkManager instance {get; set;}

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("Overriding old battlenetworkmanager! Make sure it's only loaded once!");
        }

        instance = this;

        // If battle scene is already active, make sure host is started 
        // (only really used for testing straight into battle scene and skipping battle setup)
        if (BattleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            BattleLobbyManager.StartNetworkManagerHost();
        }
    }
}