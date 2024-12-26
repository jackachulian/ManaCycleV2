using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSetupManager : MonoBehaviour
{
    /// <summary>
    /// The BattleSetupManager for the current battle setup scene. Only one should exist at a time, a warning will be raised if there is more than one.
    /// </summary>
    public static BattleSetupManager instance {get; set;}

    /// <summary>
    /// True if the current battle setup is for an online match. False if it is for a local only match.
    /// Note: both online and offline will use Netcode, but offline will only have one player being the host.
    /// </summary>
    public static bool online {get; set;} = true;

    /// <summary>
    /// Current session this client is connected to.
    /// </summary>
    public ISession current_session {get; set;}

    [SerializeField] private ConnectionMenu connectMenu;

    [SerializeField] private CharacterSelectMenu characterSelectMenu;

    public enum BattleSetupState {
        CONNECT_MENU,
        CHARACTER_SELECT
    }
    /// <summary>
    /// THe current state of the battle setup menu, which can be the connection host/join menu, or the character select screen.
    /// </summary>
    public BattleSetupState state;

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("A new BattleSetupManager has replaced the old one! Make sure there is only one BattleSetupManager in the scene.");
        }

        instance = this;
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
        current_session = session;
    }
}
