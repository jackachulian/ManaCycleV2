using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contains the player ID, username, and other info.
/// ID is a number 0-3. Tells the battle manager which board it should connect the player's inputs to.
/// </summary>
public class BattlePlayer : NetworkBehaviour {
    /// <summary>
    /// A number 0-3. Determines which board this is connected to.
    /// </summary>
    public int id {get; set;}

    /// <summary>
    /// the Unity engine's Input System player input manager
    /// </summary>
    private PlayerInput playerInput {get; set;}

    /// <summary>
    /// Custom script that responds to the inputs sent from the Player Input component and sends them to the board
    /// </summary>
    private BattleInputController playerInputController;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerInput = GetComponent<PlayerInput>();
        playerInputController = GetComponent<BattleInputController>();

        // enable input if the client owns this player (always true in singleplayer, caries in multiplayer)
        if (IsOwner) {
            EnableUserInput();
        } else {
            DisableUserInput();
        }

        // TODO: make this work with 4 players
        id = IsHost ? 0 : 1;

        // If already in battle setup mode, connect the board
        if (BattleSetupManager.instance) {
            BattleSetupConnectPanel();
        }

        // If already in battle mode, connect the board
        if (BattleManager.instance) {
            BattleConnectBoard();
        } else {
            DisableBattleInputs();
        }
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
        // TODO: ideally, make this work when there is up to 4 players. or online could just be 1v1s
        int boardIndex = id;
        playerInputController.board = BattleManager.instance.GetBoardByIndex(boardIndex);
        Debug.Log(this+" connected to "+playerInputController.board);
    }

    public void EnableUserInput() {
        playerInput.enabled = true;
    }

    public void DisableUserInput() {
        playerInput.enabled = false;
    }

    public void EnableBattleInputs() {
        playerInput.actions.FindActionMap("Battle").Enable();
    }

    public void DisableBattleInputs() {
        playerInput.actions.FindActionMap("Battle").Disable();
    }
}