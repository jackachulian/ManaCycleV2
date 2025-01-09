using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ConnectionMenuUI : MonoBehaviour {
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

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