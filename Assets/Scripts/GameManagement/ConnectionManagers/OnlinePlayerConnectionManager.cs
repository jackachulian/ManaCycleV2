using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlinePlayerConnectionManager : IServerPlayerConnectionManager {
    private bool isListening = false;

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("OnlinePlayerConnectionManager is already listening!");
            return;
        }

        if (!NetworkManager.Singleton.IsListening) {
            Debug.LogError("NetworkManager is not listening; this is needed for the OnlinePlayerConnectionManager to listen for OnClientConnected");
            return;
        }

        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Trying to setup online player connection manager on a non-server!");
            return;
        }

        isListening = true;
        NetworkManager.Singleton.OnClientConnectedCallback += ServerOnClientConnected;

        Debug.Log("OnlinePlayerConnectionManager is listening for players");
    }

    public void StopListeningForPlayers() {
        if (!isListening) return;

        isListening = false;
        NetworkManager.Singleton.OnClientConnectedCallback -= ServerOnClientConnected;
    }

    public void ServerOnClientConnected(ulong clientId) {
        // onPlayerConnected.Invoke(clientId);
    }

    public void OnPlayerSpawned(Player player) {
        
    }

    public void OnPlayerDespawned(Player player) {
        
    }
}