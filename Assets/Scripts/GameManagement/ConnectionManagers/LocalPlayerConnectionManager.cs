using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerConnectionManager : IServerPlayerConnectionManager {
    private bool isListening = false;

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("LocalPlayerConnectionManager is already listening!");
            return;
        }

        GameManager.Instance.playerInputManager.onPlayerJoined += PlayerJoined;
        GameManager.Instance.playerInputManager.onPlayerLeft += PlayerLeft;
        GameManager.Instance.playerInputManager.EnableJoining();

        isListening = true;

        Debug.Log("LocalPlayerConnectionManager is listening for players");
    }

    public void StopListeningForPlayers()
    {
        GameManager.Instance.playerInputManager.DisableJoining();
        GameManager.Instance.playerInputManager.onPlayerJoined -= PlayerJoined;
        GameManager.Instance.playerInputManager.onPlayerLeft -= PlayerLeft;

        isListening = false;
    }

    public void PlayerJoined(PlayerInput playerInput) {
        Debug.Log("PlayerInput joined locally: "+playerInput);
        Player player = playerInput.GetComponent<Player>();
        ulong playerId = (ulong)playerInput.devices[0].deviceId; // use device ID as the player ID

        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
        player.playerId.Value = playerId;
    }

    public void PlayerLeft(PlayerInput playerInput) {
        Debug.Log("PlayerInput left locally: "+playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.GetComponent<NetworkObject>().Despawn(destroy: true);
    }
}