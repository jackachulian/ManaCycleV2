using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSetupManager : MonoBehaviour
{
    /// <summary>
    /// Stores shared battle lobby dependencies
    /// </summary>
    [SerializeField] private BattleLobbyManager battleLobbyManager;

    [SerializeField] private ConnectionMenu connectMenu;

    [SerializeField] private CharacterSelectMenu _characterSelectMenu;
    public CharacterSelectMenu characterSelectMenu => _characterSelectMenu;

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
            if (battleLobbyManager.battleSetupManager == this) {
                Debug.LogWarning("This battlesetupmanager woke up twice?");
            } else {
                Debug.LogWarning("Duplicate BattleSetupManager! Destroying the old one");
                Destroy(battleLobbyManager.battleSetupManager.gameObject);
            }
        }

        battleLobbyManager.battleSetupManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE_SETUP;
        
        if (!battleLobbyManager.networkManager.IsHost && !battleLobbyManager.networkManager.IsClient) {
            battleLobbyManager.battleType = BattleLobbyManager.BattleType.NONE;
        }
    }

    private void Start() {
        // If a game is already active, show the char select menu. If not, show the connection menu.
        if (battleLobbyManager.battleType != BattleLobbyManager.BattleType.NONE) {
            ShowCharSelect();
        } else {
            ShowConnectionMenu();
        }
    }

    /// <summary>
    /// Display the connection menu for connecting to online game sessions.
    /// </summary>
    public void ShowConnectionMenu() {
        Debug.Log("Showing connection menu");
        state = BattleSetupState.CONNECT_MENU;
        battleLobbyManager.battleType = BattleLobbyManager.BattleType.NONE;
        connectMenu.gameObject.SetActive(true);
        characterSelectMenu.HideUI();
        battleLobbyManager.battlePlayerInputManager.DisableJoining();
        battleLobbyManager.battlePlayerInputManager.DisconnectAllPlayers();
    }

    /// <summary>
    /// Initialize and show the char select menu.
    /// </summary>
    public void ShowCharSelect() {
        Debug.Log("Showing char select menu");
        state = BattleSetupState.CHARACTER_SELECT;
        connectMenu.gameObject.SetActive(false);
        characterSelectMenu.InitializeBattleSetup();
        if (battleLobbyManager.battleType == BattleLobbyManager.BattleType.LOCAL_MULTIPLAYER) {
            battleLobbyManager.battlePlayerInputManager.EnableJoining();
        }
    }
}
