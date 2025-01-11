using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SingleplayerConnectionManager : IServerPlayerConnectionManager {
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

        // uhhs this is pretty much an empty calss now since i realized StartHost() will just creat ethe player so nothing needed here

        Debug.Log("Single player created");
    }

    public void StopListeningForPlayers()
    {
        // try to remove the player that the client controls
        isListening = false;
    }

    public void OnPlayerSpawned(Player player) {
        
    }

    public void OnPlayerDespawned(Player player) {
        
    }
}