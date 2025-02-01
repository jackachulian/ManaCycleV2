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

        // note: player 0 (local client's palyer) should be created by now by NetworkManager.StartHost auto spawning a player.

        // if there is a level, set up its battlers
        if (GameManager.Instance.level) {
            GameManager.Instance.level.SetupBattlers();
        }

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