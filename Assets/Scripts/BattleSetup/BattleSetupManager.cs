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
            if (battleLobbyManager.battleSetupManager == this) {
                Debug.LogWarning("This battlesetupmanager woke up twice?");
            } else {
                Debug.LogWarning("Duplicate BattleSetupManager! Destroying the old one");
                Destroy(battleLobbyManager.battleSetupManager.gameObject);
            }
        }

        battleLobbyManager.battleSetupManager = this;
        battleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE_SETUP;
    }

    private void Start() {
        // TODO: skip the connection screen if in singleplayer.
        ShowConnectionMenu();
    }

    /// <summary>
    /// Display the connection menu for connecting to online game sessions.
    /// </summary>
    public void ShowConnectionMenu() {
        Debug.Log("Showing connection menu");
        state = BattleSetupState.CONNECT_MENU;
        connectMenu.gameObject.SetActive(true);
        characterSelectMenu.HideUI();
        battleLobbyManager.battlePlayerInputManager.DisableJoining();
        battleLobbyManager.battlePlayerInputManager.DisconnectAllPlayers();
    }

    /// <summary>
    /// Initialize and show the char select menu.
    /// </summary>
    public void InitializeCharSelect() {
        Debug.Log("Showing char select menu");
        state = BattleSetupState.CHARACTER_SELECT;
        connectMenu.gameObject.SetActive(false);
        characterSelectMenu.InitializeBattleSetup();
        if (battleLobbyManager.battleType == BattleLobbyManager.BattleType.LOCAL_MULTIPLAYER) {
            battleLobbyManager.battlePlayerInputManager.EnableJoining();
        }
    }
}
