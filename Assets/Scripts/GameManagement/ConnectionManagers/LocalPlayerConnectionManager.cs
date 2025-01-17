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

    /// <summary>
    /// Only used to spawn AI players
    /// </summary>
    [SerializeField]  private Player aiPlayerPrefab;

    void Awake() {
        playerInputManager = GetComponent<PlayerInputManager>();
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            AddCPUPlayer();
        }
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
        player.playerId.Value = (ulong)player.playerInput.playerIndex;
    }

    public void OnPlayerLeft(PlayerInput playerInput) {
        Debug.Log("PlayerInput left locally: "+playerInput);
        Player player = playerInput.GetComponent<Player>();
        
        // only despawn if still spawned, since network closing should try to auto destroy these, but player disconnecting their controller wont
        if (player.IsSpawned) player.GetComponent<NetworkObject>().Despawn(destroy: true);
    }

    public void OnPlayerDespawned(Player player) {
        
    }

    public void AddCPUPlayer() {
        // disabling this on the prefab will make it so the PlayerInputManager doesn't try to assign this a device when it is instantiated
        aiPlayerPrefab.GetComponent<PlayerInput>().enabled = false;

        Player player = Instantiate(aiPlayerPrefab);
        player.isCpu = true;
        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
        player.DisableUserInput();
        player.username.Value = "CPU";
        player.EnableBattleAI();
    }
}