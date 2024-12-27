using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSetupManager : MonoBehaviour
{
    /// <summary>
    /// Stores shared battle lobby dependencies
    /// </summary>
    [SerializeField] public BattleLobbyManager battleLobbyManager;

    [SerializeField] private ConnectionMenu connectMenu;

    [SerializeField] public CharacterSelectMenu characterSelectMenu;

    public enum BattleSetupState {
        CONNECT_MENU,
        CHARACTER_SELECT
    }
    /// <summary>
    /// THe current state of the battle setup menu, which can be the connection host/join menu, or the character select screen.
    /// </summary>
    public BattleSetupState state;

    private void Awake() {
        if (battleLobbyManager.battleSetupManager != null) {
            Debug.LogWarning("Duplicate BattleSetupManager! Destroying the old one.");
            Destroy(battleLobbyManager.battleSetupManager.gameObject);
        }

        battleLobbyManager.battleSetupManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE_SETUP;
        battleLobbyManager.StartNetworkManagerScene();

        
        // start off with joins disabled. Enable it once the char select panel is shown in local multiplayer
        battleLobbyManager.StartPlayerInputManager();
        battleLobbyManager.battlePlayerInputManager.DisableJoining();
    }

    private void Start() {
        // TODO: skip the connection screen if in singleplayer.
        ShowConnectionMenu();

        // Connect all players to their battle setup
        var players = FindObjectsByType<BattlePlayer>(FindObjectsSortMode.None);

        foreach (BattlePlayer player in players) {
            player.BattleSetupConnectPanel();
            player.DisableBattleInputs();
        }
    }

    /// <summary>
    /// Display the connection menu for connecting to online game sessions.
    /// </summary>
    public void ShowConnectionMenu() {
        state = BattleSetupState.CONNECT_MENU;
        connectMenu.gameObject.SetActive(true);
        characterSelectMenu.gameObject.SetActive(false);
        battleLobbyManager.battlePlayerInputManager.DisableJoining();
        battleLobbyManager.battlePlayerInputManager.DisconnectAllPlayers();
    }

    /// <summary>
    /// Initialize and show the char select menu.
    /// </summary>
    public void InitializeCharSelect() {
        state = BattleSetupState.CHARACTER_SELECT;
        connectMenu.gameObject.SetActive(false);
        characterSelectMenu.InitializeBattleSetup();
        battleLobbyManager.battlePlayerInputManager.EnableJoining();
    }
}
