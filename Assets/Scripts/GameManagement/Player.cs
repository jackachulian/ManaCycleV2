using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Contains the player ID, username, and other info.
/// ID is a number 0-3. Tells the battle manager which board it should connect the player's inputs to.
/// </summary>
public class Player : NetworkBehaviour {
    /// <summary>
    /// the ID of this player, assigned by the player manager.
    /// </summary>
    public NetworkVariable<ulong> playerId = new NetworkVariable<ulong>();

    /// <summary>
    /// A number 0-3. Determines which CharPortrait (character select) or Board (battle) this is connected to.
    /// </summary>
    public NetworkVariable<int> boardIndex = new NetworkVariable<int>(-1);

    /// <summary>
    /// The client's username. Set by the client when joining.
    /// </summary>
    public NetworkVariable<FixedString128Bytes> username = new NetworkVariable<FixedString128Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
    /// Input handler for the char select scene
    /// </summary>
    public CharSelectInputHandler charSelectInputHandler {get; private set;}

    /// <summary>
    /// Input handler for the battle scene
    /// </summary>
    public BattleInputHandler battleInputHandler {get; private set;}

    /// <summary>
    /// Used for layer-specific ui events during charselect on the Options menu
    /// </summary>
    public MultiplayerEventSystem multiplayerEventSystem {get; private set;}

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        charSelectInputHandler = GetComponent<CharSelectInputHandler>();
        battleInputHandler = GetComponent<BattleInputHandler>();
        multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
        boardIndex.OnValueChanged += OnBoardIndexChanged;
    }

    public override void OnNetworkSpawn()
    {
        // make sure this persists between the charselect and the battle scene!
        DontDestroyOnLoad(gameObject);

        // only enable input if the client owns this. (always true in singleplayer, varies in multiplayer)
        if (IsOwner) {
            EnableUserInput();
        } else {
            DisableUserInput();
        }

        // only enable the battle input upon joining if not in the charselect menu
        // battle inputs will be enabled when leaving this scene to go to the battle scene
        if (GameManager.Instance.currentGameState != GameManager.GameState.CharSelect) {
            EnableBattleInputs();
        } else {
            DisableBattleInputs();
        }

        // If this is the server, add to the server player manager
        if (NetworkManager.Singleton.IsServer) {
            GameManager.Instance.playerManager.ServerAddPlayer(this);
        }

        // call playerspawned on all clients (including host)
        GameManager.Instance.playerManager.PlayerSpawned(this);
    }

    /// <summary>
    /// Called when the board index changes.
    /// Board index should only be changed during char select phase - this will raise an error if not in charselect phase.
    /// </summary>
    public void OnBoardIndexChanged(int previous, int current) {
        if (GameManager.Instance.currentGameState != GameManager.GameState.CharSelect) {
            Debug.LogError("Board index changed while not in char select! Only change boardIndex while in charselect!");
            return;
        }

        Debug.Log("Assigning player with ID "+playerId.Value+" to board number "+current+" (previous: "+previous+")");

        // Assign player to the charselector of the given index
        CharSelector selector = CharSelectManager.Instance.GetCharSelector(boardIndex.Value);
        selector.AssignPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        // If this is the server, remove from the server player manager
        if (NetworkManager.Singleton.IsServer) {
            GameManager.Instance.playerManager.ServerRemovePlayer(this);
        }

        GameManager.Instance.playerManager.PlayerDespawned(this);
    }

    public void EnableUserInput() {
        Debug.Log("Enabling user input on "+this);
        playerInput.enabled = true;
    }

    public void DisableUserInput() {
        Debug.Log("Disabling user input on "+this);
        playerInput.enabled = false;
    }

    public async void EnableBattleInputs() {
        // has to be after a delay because unity is jank
        await Awaitable.NextFrameAsync();
        Debug.Log("Enabled battle inputs on "+this);
        playerInput.actions.FindActionMap("Battle", true).Enable();
    }

    public async void DisableBattleInputs() {
        await Awaitable.NextFrameAsync();
        Debug.Log("Disabled battle inputs on "+this);
        playerInput.actions.FindActionMap("Battle", true).Disable();
    }
}