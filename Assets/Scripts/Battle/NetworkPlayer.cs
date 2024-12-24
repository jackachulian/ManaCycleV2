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
    private BattleInputController playerInputController;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = false; // start false just so it doesn't steal any input devices when it shouldn't
        playerInputController = GetComponent<BattleInputController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // enable input if the client owns this network player
        playerInput.enabled = IsOwner;

        // If already in battle setup mode, connect the board
        if (BattleSetupManager.instance) BattleSetupConnectPanel();

        // If already in battle mode, connect the board
        if (BattleManager.instance) BattleConnectBoard();
    }

    /// <summary>
    /// Connect the player to their battle setup player panel based on their network id.
    /// </summary>
    public void BattleSetupConnectPanel() {
        // TODO: implement after battle setup scene is implemented
    }

    /// <summary>
    /// Connect the player input to the appropriate board based on network id.
    /// </summary>
    public void BattleConnectBoard() {
        int boardIndex = IsHost ? 0 : 1;
        playerInputController.board = BattleManager.instance.GetBoardByIndex(boardIndex);
    }

    [Rpc(SendTo.NotOwner)]
    private void TestRpc() {
        
    }
}