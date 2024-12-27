using Unity.Netcode;
using UnityEngine;

public class BattleNetworkManager : NetworkManager {
    /// <summary>
    /// Initialize via the BattleLobbyManager.
    /// </summary>
    public void Initialize(BattleLobbyManager battleLobbyManager) {
        // If battle scene is already active, make sure host is started
        // (only really used for testing straight into battle scene and skipping battle setup)
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            battleLobbyManager.StartNetworkManagerHost();
        }
    }
}