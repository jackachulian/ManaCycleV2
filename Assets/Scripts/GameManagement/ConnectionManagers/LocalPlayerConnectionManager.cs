using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalPlayerConnectionManager : IServerPlayerConnectionManager {
    public event Action<ulong> onPlayerConnected;
    public event Action<ulong> onPlayerDisconnected;

    private bool isListening = false;

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("OnlinePlayerConnectionManager is already listening!");
            return;
        }

        isListening = true;
        // TODO: enable player input manager, listen for player input new devices
    }

    public void StopListeningForPlayers()
    {
        isListening = false;
        // TODO: disable player input manager
    }

    public void OnPlayerJoined(ulong deviceId) {
        onPlayerConnected.Invoke(deviceId);
    }
}