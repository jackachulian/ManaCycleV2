using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SingleplayerConnectionManager : IServerPlayerConnectionManager {
    public event Action<ulong> onPlayerConnected;
    public event Action<ulong> onPlayerDisconnected;

    private bool isListening = false;

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("Single player already spawned");
            return;
        }

        // don't actually listenfor anything
        // but create a single player of id 0 that the player will control
        isListening = true;
        onPlayerConnected.Invoke(0);
    }

    public void StopListeningForPlayers()
    {
        // try to remove the player that the client controls
        isListening = false;
        onPlayerDisconnected.Invoke(0);
    }
}