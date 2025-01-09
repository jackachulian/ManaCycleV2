using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Should be placed on the same gameobject as the PlayerInputManager
/// </summary>
public class LocalPlayerConnectionManager : MonoBehaviour, IServerPlayerConnectionManager {
    private bool isListening = false;

    private PlayerInputManager playerInputManager;

    void Awake() {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("LocalPlayerConnectionManager is already listening!");
            return;
        }

        playerInputManager.EnableJoining();
        isListening = true;

        Debug.Log("LocalPlayerConnectionManager is listening for players");
    }

    public void StopListeningForPlayers()
    {
        playerInputManager.DisableJoining();
        isListening = false;
    }

    public void OnPlayerJoined(PlayerInput playerInput) {
        Debug.Log("PlayerInput joined locally: "+playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
    }

    // Set the player's player ID after they are spawned.
    public void OnPlayerSpawned(Player player) {
        ulong playerId = (ulong)player.GetComponent<PlayerInput>().devices[0].deviceId; // use device ID as the player ID
        player.playerId.Value = playerId;
    }

    public void OnPlayerLeft(PlayerInput playerInput) {
        Debug.Log("PlayerInput left locally: "+playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.GetComponent<NetworkObject>().Despawn(destroy: true);
    }

    public void OnPlayerDespawned(Player player) {
        
    }
}