using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour {
    /// <summary>
    /// the Unity engine's Input System player input manager
    /// </summary>
    private PlayerInput playerInput;

    /// <summary>
    /// Custom script that responds to the inputs sent from the Player Input component and sends them to the board
    /// </summary>
    private PlayerInputController playerInputController;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerInputController = GetComponent<PlayerInputController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // enable input if the client owns this network player
        playerInput.enabled = IsOwner;

        int boardIndex = IsHost ? 0 : 1;
        playerInputController.board = BattleManager.instance.GetBoardByIndex(boardIndex);
    }
}