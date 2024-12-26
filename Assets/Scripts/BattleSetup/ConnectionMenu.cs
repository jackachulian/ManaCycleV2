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
        // make sure there is a networkmanager before a session can be started
        StartNetworkManagerScene();

        // online by default when connection menu is open and join buttons may be pressed, will change to offline if singleplayer is pressed
        BattleSetupManager.online = true; 
    }

    /// <summary>
    /// Go to the character select menu when a session is created/joined.
    /// </summary>
    public void OnSessionJoined(ISession session) {
        Debug.Log("Session has ben joined: "+session.Code);

        BattleSetupManager.instance.SetSession(session);
        BattleSetupManager.instance.InitializeCharSelect();
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
        BattleSetupManager.instance.ShowConnectionMenu();
        foreach (Selectable s in disableWhileJoining) {
            s.interactable = true;
        }
    }

    /// <summary>
    /// Ensures that the network manager scene is loaded additively.
    /// </summary>
    public void StartNetworkManagerScene() {
        if (BattleNetworkManager.instance == null) {
            Debug.Log("Starting network management scene");
            SceneManager.LoadScene("NetworkManagement", LoadSceneMode.Additive);
        } else {
            Debug.Log("Skipping network management scene load, battle network manager already present");
        }
    }

    /// <summary>
    /// TODO: probably should move this to the home menu
    /// </summary>
    public void OnSinglePlayerPressed() {
        BattleSetupManager.online = false;

        // all singleplayer will be a local host but deny all incoming connections
        BattleNetworkManager.instance.StartHost();


        BattleSetupManager.instance.InitializeCharSelect();
    }
}