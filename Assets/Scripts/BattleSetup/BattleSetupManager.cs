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
    }

    public void ShowConnectionMenu() {
        state = BattleSetupState.CONNECT_MENU;
        connectMenu.gameObject.SetActive(true);
        characterSelectMenu.gameObject.SetActive(false);
    }

    public void ShowCharSelectMenu() {
        state = BattleSetupState.CHARACTER_SELECT;
        connectMenu.gameObject.SetActive(false);
        characterSelectMenu.gameObject.SetActive(true);
    }

    public void SetSession(ISession session) {
        current_session = session;
    }
}
