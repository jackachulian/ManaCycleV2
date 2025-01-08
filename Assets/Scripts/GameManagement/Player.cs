using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contains the player ID, username, and other info.
/// ID is a number 0-3. Tells the battle manager which board it should connect the player's inputs to.
/// </summary>
public class Player : NetworkBehaviour {
    /// <summary>
    /// the ID of this player, assigned by the player manager.
    /// </summary>
    public NetworkVariable<ulong> playerId {get; set;}

    /// <summary>
    /// The client's username. Set by the client when joining.
    /// </summary>
    public NetworkVariable<FixedString128Bytes> username = new NetworkVariable<FixedString128Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// A number 0-3. Determines which board this is connected to.
    /// </summary>
    public NetworkVariable<int> boardIndex = new NetworkVariable<int>(-1);

    /// <summary>
    /// True when this player is ready to start the game, and false when still choosing their character/settings.
    /// (The client can write to this value, not just the server)
    /// </summary>
    public NetworkVariable<bool> ready = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// the Unity engine's Input System player input manager
    /// </summary>
    private PlayerInput playerInput {get; set;}

    /// <summary>
    /// Custom script that responds to the inputs sent from the Player Input component and sends them to the board
    /// </summary>
    private BattleInputController playerInputController;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerInputController = GetComponent<BattleInputController>();
        DisableUserInput();
    }

    public override void OnNetworkSpawn()
    {
        // only enable input if the client owns this. (always true in singleplayer, varies in multiplayer)
        if (IsOwner) {
            EnableUserInput();
        } else {
            DisableUserInput();
        }

        DontDestroyOnLoad(gameObject);
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