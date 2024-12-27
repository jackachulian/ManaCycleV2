using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionMenu : MonoBehaviour
{
    /// <summary>
    /// Selectables in this array will be temporarily un-interactable while attempting to join a session.
    /// </summary>
    [SerializeField] private Selectable[] disableWhileJoining;

    private void Start() {
        // ONLNE_MULTIPLAYER by default when connection menu is open and join buttons may be pressed, 
        // will change to LOCAL_MULTIPLAYER if offline button is pressed
        BattleLobbyManager.battleType = BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER;
    }

    /// <summary>
    /// Go to the character select menu when a session is created/joined.
    /// </summary>
    public void OnSessionJoined(ISession session) {
        Debug.Log("Session has ben joined: "+session.Code);

        BattleLobbyManager.current_session = session;
        BattleLobbyManager.battleSetupManager.InitializeCharSelect();
    }

    /// <summary>
    /// Disables the UI while joining a session.
    /// </summary>
    public void OnJoiningSession() {
        foreach (Selectable s in disableWhileJoining) {
            s.interactable = false;
        }

        // TODO: show loading icon
    }

    /// <summary>
    /// Re-enables the ui when session failed to join.
    /// </summary>
    public void OnFailedToJoinSession(SessionException exception) {
        foreach (Selectable s in disableWhileJoining) {
            s.interactable = true;
        }

        // TODO: show error message and hide loading icon
        Debug.LogError("Failed to join session: "+exception);
    }

    /// <summary>
    /// Re-show the connection menu when a session is terminated.
    /// </summary>
    public void OnSessionLeft() {
        BattleLobbyManager.battleSetupManager.ShowConnectionMenu();
        foreach (Selectable s in disableWhileJoining) {
            s.interactable = true;
        }
    }

    

    /// <summary>
    /// TODO: probably should move this to the home menu
    /// </summary>
    public void OnSinglePlayerPressed() {
        BattleLobbyManager.battleType = BattleLobbyManager.BattleType.LOCAL_MULTIPLAYER;

        // all singleplayer will be a local host but deny all incoming connections
        BattleNetworkManager.instance.StartHost();


        BattleLobbyManager.battleSetupManager.InitializeCharSelect();
    }
}