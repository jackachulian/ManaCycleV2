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
}