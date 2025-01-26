using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayConnectionManager : IServerPlayerConnectionManager {
    private bool isListening = false;
    
    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("Single player already spawned");
            return;
        }

        isListening = true;

        BattleManager.Instance.replayManager.LoadFromFile();
        GameManager.Instance.SetBattleData(BattleManager.Instance.replayManager.replayData.battleData);
        GameManager.Instance.playerManager.AddReplayPlayers(BattleManager.Instance.replayManager.replayData);
        

        Debug.Log("Single player created");
    }

    public void StopListeningForPlayers()
    {
        isListening = false;
    }

    public void OnPlayerSpawned(Player player) {
        
    }

    public void OnPlayerDespawned(Player player) {
        
    }
}