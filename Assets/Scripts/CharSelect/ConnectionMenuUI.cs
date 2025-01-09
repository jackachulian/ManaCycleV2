using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConnectionMenuUI : MonoBehaviour {
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    [SerializeField] private Button _refreshButton;
    public Button refreshButton => _refreshButton;

    // Connection menu needs a seperate event system because the char select menu has an event system for each connected player.
    [SerializeField] private EventSystem _connectionMenuEventSystem;
    public EventSystem connectionMenuEventSystem => _connectionMenuEventSystem;

    void Start() {
        foreach (Transform t in lobbyListParent) {
            Destroy(t.gameObject);
        }
    }

    // This may or may not remain in the final versions, 
    // text to show current lobby setup / services initialization status, also good for debugging
    public void ShowStatus(string status) {
        Debug.Log(status);
        statusText.text = status;
    }

    public void ShowLobbies(List<Lobby> lobbies) {
        foreach (Transform t in lobbyListParent) {
            Destroy(t.gameObject);
        }

        foreach (Lobby lobby in lobbies) {
            LobbyItem newLobbyItem = Instantiate(lobbyItemPrefab, lobbyListParent);
            newLobbyItem.SetLobby(lobby);
        }
    }
}