using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour {
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyPlayerCountText;

    private Lobby lobby;

    public void SetLobby(Lobby lobby) {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        lobbyPlayerCountText.text = lobby.Players.Count+"/"+lobby.MaxPlayers;
    }

    public void OnPressed() {
        LobbyManager.Instance.JoinLobby(lobby);
    }
}