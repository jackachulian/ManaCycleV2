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
public class BattlePlayer : NetworkBehaviour {
    /// <summary>
    /// Stores shared battle lobby dependencies
    /// </summary>
    [SerializeField] private BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// Used only in non-online mode. the ID of this player, assigned by the player input manager.
    /// </summary>
    public ulong localPlayerId {get; set;}

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

    /// <summary>
    /// The current setup panel assigned to this player
    /// </summary>
    private BattleSetupPlayerPanel playerPanel;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerInputController = GetComponent<BattleInputController>();

        DisableUserInput();
    }

    public override void OnNetworkSpawn()
    {
        boardIndex.OnValueChanged += OnBoardIndexChanged;
        username.OnValueChanged += OnUsernameChanged;

        if (IsServer) {
            boardIndex.Value = -1;
        }
        
        // Register this player with the player manager
        battleLobbyManager.playerManager.AddPlayer(GetId(), this);

        ConnectToBoard(boardIndex.Value);

        // Set username asynchrously, if this is the local player
        if (IsLocalPlayer) {
            SetUsernameAsync();
        }

        // only enable input if this is the local player (always true in singleplayer, varies in multiplayer)
        if (IsLocalPlayer) {
            EnableUserInput();
        } else {
            DisableUserInput();
        }

        // if in battle setup, disable battle inputs

        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE_SETUP) {
            // Let CharSelect know this player now exists
            // Will assign this player a panel and listen for when it is ready to start the game
            var charSelect = battleLobbyManager.battleSetupManager.characterSelectMenu.characterSelectNetworkBehaviour;
            charSelect.OnPlayerJoined(GetId());

            DisableBattleInputs();
            // BattleSetupConnectPanel();
        }
        // // If already in battle mode, connect the board
        // else if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
        //     BattleConnectBoard();
        // }

        DontDestroyOnLoad(gameObject);
    }

    
    public override void OnNetworkDespawn()
    {
        battleLobbyManager.playerManager.RemovePlayer(GetId());
    }

    /// <summary>
    /// In online multiplayer this returns the client ID
    /// in local multiplayer/singleplayer this returns the playerinput's device ID
    /// </summary>
    public ulong GetId() {
        if (battleLobbyManager.battleType == BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER) {
            return OwnerClientId;
        } else {
            // if (!playerInput) {
            //     Debug.LogError("No player input");
            //     return OwnerClientId;
            // }

            // if (playerInput.devices.Count == 0) {
            //     Debug.LogError("No devices connected to player input on "+this+"!");
            //     return OwnerClientId;
            // }

            // return (ulong)playerInput.devices[0].deviceId;

            return localPlayerId;
        }
    }

    public async void SetUsernameAsync() {
        if (UnityServices.State == ServicesInitializationState.Initialized) {
            username.Value = await AuthenticationService.Instance.GetPlayerNameAsync();
        } else {
            // TODO: may want to just wait until initialized and then set username when initialized callback. idk
            Debug.Log("Skipping username set, unity services is not initialized");
        }
    }

    public void OnUsernameChanged(FixedString128Bytes previous, FixedString128Bytes current) {
        if (playerPanel) playerPanel.SetUsername(current.ToString());
    }

    /// <summary>
    /// When board index is loaded or changes, make this player control the appropriate setup panel or board
    /// </summary>
    public void OnBoardIndexChanged(int previous, int current) {
        Debug.Log("player "+OwnerClientId+" board index changed from "+previous+" to "+current+" in battle phase "+battleLobbyManager.battlePhase);
        ConnectToBoard(current);
    }

    public void ConnectToBoard(int index) {
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE_SETUP) {
            BattleSetupConnectPanel(index);
        } else if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            BattleConnectBoard(index);
        }
    }

    public void DisconnectFromBattleBoard() {
        playerInputController.board = null;
    }

    /// <summary>
    /// Connect the player to their battle setup player panel based on the passed index.
    /// </summary>
    public void BattleSetupConnectPanel(int index) {
        // don't connect if value is the default -1
        if (index < 0) {
            Debug.LogError("Trying to connect battleplayer to setup panel but index is invalid: "+index);
            return;
        };

        BattleSetupPlayerPanel panel = battleLobbyManager.battleSetupManager.characterSelectMenu.GetPanel(index);

        // If panel does not change or is still null, return
        if (playerPanel == panel) return;

        // If a non-null panel is already assigned, unassign this player from it
        if (playerPanel) {
            playerPanel.AssignPlayer(null);
        }

        playerPanel = panel;

        // If new panel is non-null, assign this player to it
        if (playerPanel) {
            playerPanel.AssignPlayer(this);
        }
    }

    /// <summary>
    /// Connect the player input to the appropriate board based on the passed index.
    /// </summary>
    public void BattleConnectBoard(int index) {
        if (index < 0) {
            Debug.LogWarning("Trying to connect battleplayer to board but board index is invalid: "+index);
            return;
        };

        // TODO: ideally, make this work when there is up to 4 players. or online could just be 1v1s
        Board board = battleLobbyManager.battleManager.GetBoardByIndex(index);
        playerInputController.board = board;
        board.onPlayerConnected.Invoke(this);
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