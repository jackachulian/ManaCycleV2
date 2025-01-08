using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SingleplayerConnectionManager : IServerPlayerConnectionManager {
    public event Action<Player, ulong> onPlayerConnected;
    public event Action<Player, ulong> onPlayerDisconnected;

    private bool isListening = false;

    /// <summary>
    /// Prefab used to spawn the player
    /// </summary>
    [SerializeField] private Player playerPrefab;

    /// <summary>
    /// The spawned player instance in the scene
    /// </summary>
    private Player player;

    public void StartListeningForPlayers()
    {
        if (isListening) {
            Debug.LogWarning("Single player already spawned");
            return;
        }

        // don't actually listenfor anything
        // but create a single player of id 0 that the player will control
        isListening = true;
        player = GameObject.Instantiate(playerPrefab);
        onPlayerConnected.Invoke(player, 0);

        Debug.Log("Single player created");
    }

    public void StopListeningForPlayers()
    {
        // try to remove the player that the client controls
        isListening = false;
        onPlayerDisconnected.Invoke(player, 0);
    }
}