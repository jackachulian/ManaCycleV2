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

    private CanvasGroup canvasGroup;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Go to the character select menu when a session is created/joined.
    /// </summary>
    public void OnSessionJoined(ISession session) {
        Debug.Log("Session has ben joined: "+session.Code);

        BattleSetupManager.instance.SetSession(session);
        BattleSetupManager.instance.ShowCharSelectMenu();
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
        if (!NetworkManager.Singleton) {
            SceneManager.LoadScene("NetworkManager", LoadSceneMode.Additive);
        }
    }
}
