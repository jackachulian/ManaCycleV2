using Unity.Netcode;
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
    [SerializeField] public BattleLobbyManager battleLobbyManager;

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

    public override void OnNetworkSpawn()
    {
        // start off as board -1 (no board)
        if (IsServer) {
            boardIndex.Value = -1;
        }

        battleLobbyManager.battleNetworkManager.AddPlayer(OwnerClientId, this);

        boardIndex.OnValueChanged += OnBoardIndexChanged;

        playerInput = GetComponent<PlayerInput>();
        playerInputController = GetComponent<BattleInputController>();

        // enable input if the client owns this player (always true in singleplayer, varies in multiplayer)
        if (IsOwner) {
            EnableUserInput();
        } else {
            DisableUserInput();
        }

        // if in battle setup, disable battle inputs
        // (BoardIndex's OnValueChanged will connect the board to the appropriate panel)
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE_SETUP) {
            DisableBattleInputs();
            BattleSetupConnectPanel();
        }
        // If already in battle mode, connect the board
        else if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE) {
            BattleConnectBoard();
        }

        // If this is the server, assign to the next available slot
        if (battleLobbyManager.battleNetworkManager.IsServer) {
            battleLobbyManager.battleSetupManager.characterSelectMenu.characterSelectNetworkBehaviour.AssignClientToNextAvailablePanel(OwnerClientId);
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        boardIndex.OnValueChanged -= OnBoardIndexChanged;

        battleLobbyManager.battleNetworkManager.RemovePlayer(OwnerClientId);
    }

    /// <summary>
    /// When board index is loaded or changes, make this player control the appropriate setup panel or board
    /// </summary>
    public void OnBoardIndexChanged(int previous, int current) {
        if (battleLobbyManager.battlePhase == BattleLobbyManager.BattlePhase.BATTLE_SETUP) {
            BattleSetupConnectPanel();
        }
    }

    /// <summary>
    /// Connect the player to their battle setup player panel based on their network id.
    /// </summary>
    public void BattleSetupConnectPanel() {
        // don't connect if value is the default -1
        if (boardIndex.Value < 0) return;

        BattleSetupPlayerPanel panel = battleLobbyManager.battleSetupManager.characterSelectMenu.GetPanel(boardIndex.Value);

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
    /// Connect the player input to the appropriate board based on network id.
    /// </summary>
    public void BattleConnectBoard() {
        // TODO: ideally, make this work when there is up to 4 players. or online could just be 1v1s
        playerInputController.board = battleLobbyManager.battleManager.GetBoardByIndex(boardIndex.Value);
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