using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour {
    /// <summary>
    /// Contains the player ID and used to connect to the correct board
    /// </summary>
    private BattlePlayer battlePlayer;

    private void Awake() {
        battlePlayer = GetComponent<BattlePlayer>();
        battlePlayer.DisableUserInput(); // start false just so it doesn't steal any input devices when it shouldn't

        // ensures this won't be destroyed when moving bewteen the battlesetup and battle scene
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // enable input if the client owns this network player
        if (IsOwner) {
            battlePlayer.EnableUserInput();
        } else {
            battlePlayer.DisableUserInput();
        }

        // TODO: make this work with 4 players
        battlePlayer.id = IsHost ? 0 : 1;

        // If already in battle setup mode, connect the board
        if (BattleSetupManager.instance) battlePlayer.BattleSetupConnectPanel();

        // If already in battle mode, connect the board
        if (BattleManager.instance) battlePlayer.BattleConnectBoard();
    }
}