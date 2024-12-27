using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSetupManager : MonoBehaviour
{
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
        if (BattleLobbyManager.battleSetupManager != null) {
            Debug.LogWarning("Duplicate BattleSetupManager! Destroying the old one.");
            Destroy(BattleLobbyManager.battleSetupManager.gameObject);
        }

        BattleLobbyManager.battleSetupManager = this;
        BattleLobbyManager.battlePhase = BattleLobbyManager.BattlePhase.BATTLE_SETUP;
        BattleLobbyManager.StartNetworkManagerScene();
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
        BattlePlayerInputManager.instance.DisableJoining();
        BattlePlayerInputManager.instance.DisconnectAllPlayers();
    }

    /// <summary>
    /// Initialize and show the char select menu.
    /// </summary>
    public void InitializeCharSelect() {
        state = BattleSetupState.CHARACTER_SELECT;
        connectMenu.gameObject.SetActive(false);
        characterSelectMenu.InitializeBattleSetup();
        BattlePlayerInputManager.instance.EnableJoining();
    }

    public void SetSession(ISession session) {
        BattleLobbyManager.current_session = session;
    }
}
