using System;
using Battle;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
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
    /// The index of the battler this player has selected
    /// </summary>
    public NetworkVariable<int> selectedBattlerIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// True when this player has locked in their character choice
    /// </summary>
    public NetworkVariable<bool> characterChosen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// True when this player has locked in their options choices and is now ready to start the game
    /// </summary>
    public NetworkVariable<bool> optionsChosen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// Selected battler object. 
    /// Note: this is not netwok synchronized, but the value is set by CharSelect
    /// before the game is started, and this value is read in the Battle scene.
    /// </summary>
    public Battler battler;


    /// <summary>
    /// the Unity engine's Input System player input manager
    /// </summary>
    public PlayerInput playerInput {get; private set;}

    /// <summary>
    /// Input handler for the char select scene
    /// </summary>
    public CharSelectInputHandler charSelectInputHandler {get; private set;}

    /// <summary>
    /// Input handler for the battle scene
    /// </summary>
    public BattleInputHandler battleInputHandler {get; private set;}

    /// <summary>
    /// Handles RPCs during the battle scene
    /// </summary>
    public PlayerBoardNetworkBehaviour boardNetworkBehaviour {get; private set;}

    /// <summary>
    /// Used for layer-specific ui events during charselect on the Options menu
    /// </summary>
    public MultiplayerEventSystem multiplayerEventSystem {get; private set;}

    /// <summary>
    /// Is set when this player's board index is set and this player is hiooked up to one of the char selectors
    /// </summary>
    public CharSelector selector {get; private set;}


    private void Awake() {
        // If not in local multiplayer, disable the user inputs. these will be enabled if the player is owned once it spawns.
        // for local multiplayer the inputs need to be enabled when the player is added, so this would mess that up
        if (GameManager.Instance && GameManager.Instance.currentConnectionType != GameManager.GameConnectionType.LocalMultiplayer) {
            playerInput = GetComponent<PlayerInput>();
            multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
            boardNetworkBehaviour = GetComponent<PlayerBoardNetworkBehaviour>();
            DisableUserInput();
        }
    }

    public override void OnNetworkSpawn()
    {
        playerInput = GetComponent<PlayerInput>();
        charSelectInputHandler = GetComponent<CharSelectInputHandler>();
        battleInputHandler = GetComponent<BattleInputHandler>();
        multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
        boardNetworkBehaviour = GetComponent<PlayerBoardNetworkBehaviour>();

        playerId.OnValueChanged += OnPlayerIdChanged;
        boardIndex.OnValueChanged += OnBoardIndexChanged;
        username.OnValueChanged += OnUsernameChanged;
        selectedBattlerIndex.OnValueChanged += OnSelectedBattlerIndexChanged;
        characterChosen.OnValueChanged += OnCharacterChosenChanged;
        optionsChosen.OnValueChanged += OnOptionsChosenChanged;

        playerInput.onControlsChanged += OnControlsChanged;

        // make sure this persists between the charselect and the battle scene!
        DontDestroyOnLoad(gameObject);

        // only enable input if the client owns this player. (always true in singleplayer, varies in multiplayer)
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

        // In online mode, set the username on this object for others to see this player's username
        if (IsOwner && GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.OnlineMultiplayer) {
            SetUsernameAsync();
        }

        // Call this so board index can hook to board if it is already the correct and set value upon joining the game
        OnBoardIndexChanged(-1, boardIndex.Value);
    }

    public async void SetUsernameAsync() {
        username.Value = new FixedString128Bytes(await AuthenticationService.Instance.GetPlayerNameAsync());
    }

    public override void OnNetworkDespawn()
    {
        // Unassign this player from its charselector if assigned to one
        if (charSelectInputHandler.charSelector && charSelectInputHandler.charSelector.player == this) {
            charSelectInputHandler.charSelector.AssignPlayer(null);
        }

        // If this is the server, remove from the server player manager
        if (NetworkManager.Singleton.IsServer) {
            GameManager.Instance.playerManager.ServerRemovePlayer(this);
        }

        GameManager.Instance.playerManager.PlayerDespawned(this);
    }

    /// <summary>
    /// Called when the board index changes.
    /// Board index should only be changed during char select phase - this will raise an error if not in charselect phase.
    /// </summary>
    public void OnBoardIndexChanged(int previous, int current) {
        // if (GameManager.Instance.currentGameState != GameManager.GameState.CharSelect) {
        //     Debug.LogError("Board index changed while not in char select! Only change boardIndex while in charselect!");
        //     return;
        // }

        if (boardIndex.Value < 0) {
            Debug.Log("Not attaching player with ID "+playerId.Value+" to a board yet because board index is invalid: "+boardIndex.Value);
            return;
        }

        Debug.Log("Assigning player with ID "+playerId.Value+" to board number "+current+" (previous: "+previous+")");

        if (CharSelectManager.Instance) {
            AttachToCharSelector();
        } else if (BattleManager.Instance) {
            AttachToBattleBoard();
        }
    }

    /// <summary>
    /// When player ID changes, reflect the change on the char selector
    /// </summary>
    public void OnPlayerIdChanged(ulong previous, ulong current) {
        UpdateSelectorPlayerData();
    }

    /// <summary>
    /// When username changes, reflect the change on the char selector
    /// </summary>
    public void OnUsernameChanged(FixedString128Bytes previous, FixedString128Bytes current) {
        UpdateSelectorPlayerData();
    }

    public void OnSelectedBattlerIndexChanged(int previous, int current) {
        if (selector) selector.ui.UpdateSelectedBattler();
    }

    public void OnCharacterChosenChanged(bool previous, bool current) {
        if (selector) selector.ui.UpdateReadinessStatus();
    }

    public void OnOptionsChosenChanged(bool previous, bool current) {
        if (selector) {
            selector.ui.UpdateReadinessStatus();
            selector.ui.UpdateSelectedBattler();
        }
    }

    /// <summary>
    /// When controls change in local multiplayer, make sure to update player information so the new device can be shown
    /// </summary>
    public void OnControlsChanged(PlayerInput playerInput) {
        UpdateSelectorPlayerData();
    }

    /// <summary>
    /// Sets battle data on the local client, and then sends to the server the BattleData that was set.
    /// </summary>
    [Rpc(SendTo.Owner)]
    public void SetBattleDataClientRpc(BattleData battleData) {
        Debug.Log("Battle data set on client. RNG seed: "+battleData.seed);
        GameManager.Instance.SetBattleData(battleData);
        VerifyBattleDataServerRpc(battleData);
    }

    /// <summary>
    /// Only used on the server. The BattleData that was received by the client.
    /// Ensures that correct battledata was received by all clients.
    /// </summary>
    public BattleData receivedBattleData {get; set;}

    /// <summary>
    /// GameStartNetworkBehaviour listens for this to know when to check if all players have received the correct battle data
    /// </summary>
    public event Action<Player, BattleData> onBattleDataReceived;

    [Rpc(SendTo.Server)]
    public void VerifyBattleDataServerRpc(BattleData battleData) {
        Debug.Log("Battle data set on player with board index "+boardIndex.Value+". RNG seed: "+battleData.seed);
        receivedBattleData = battleData;
        onBattleDataReceived?.Invoke(this, battleData);
    }

    /// <summary>
    /// To be called whenever player specific data such as the username is changed. 
    /// If a selector is attached to the charselectinputhandler it will be updated.
    /// </summary>
    public void UpdateSelectorPlayerData() {
        if (charSelectInputHandler && charSelectInputHandler.charSelector) {
            charSelectInputHandler.charSelector.ui.UpdatePlayerName();
        }
    }

    public void EnableUserInput() {
        Debug.Log("Enabling user input on "+this);
        playerInput.enabled = true;
        multiplayerEventSystem.enabled = true;
    }

    public void DisableUserInput() {
        Debug.Log("Disabling user input on "+this);
        playerInput.enabled = false;
        multiplayerEventSystem.enabled = false;
    }

    public void EnableBattleInputs() {
        // has to be after a delay because unity is jank
        // await Awaitable.NextFrameAsync();
        Debug.Log("Enabled battle inputs on "+this);
        playerInput.actions.FindActionMap("Battle", true).Enable();
    }

    public void DisableBattleInputs() {
        // await Awaitable.NextFrameAsync();
        Debug.Log("Disabled battle inputs on "+this);
        playerInput.actions.FindActionMap("Battle", true).Disable();
    }

    public void AttachToCharSelector() {
        selector = CharSelectManager.Instance.GetCharSelectorByIndex(boardIndex.Value);
        charSelectInputHandler.SetCharSelector(selector);
        if (selector) selector.AssignPlayer(this);
    }

    /// <summary>
    /// For use in the battle scene. Attach inputs to the board with this player's current boardIndex.
    /// </summary>
    public void AttachToBattleBoard() {
        var board = BattleManager.Instance.GetBoardByIndex(boardIndex.Value);
        if (board) {
            battleInputHandler.SetBoard(board);
            board.SetPlayer(this);
            Debug.Log("Attached player "+this+" to board "+board);
        } else {
            Debug.LogError("Player "+this+" could not be attached to board, there is no board with index "+boardIndex.Value);
        }
    }
}