using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the connecting of seperate input devices as separate players in local multiplayer.
/// </summary>
public class BattlePlayerInputManager : MonoBehaviour {
    private PlayerInputManager playerInputManager;

    private List<BattlePlayer> players;

    private void Awake() {
        players = new List<BattlePlayer>();

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void OnPlayerJoined(PlayerInput playerInput) {
        BattlePlayer battlePlayer = playerInput.gameObject.GetComponent<BattlePlayer>();
        if (!battlePlayer) {
            Debug.Log("No battle player on spawned player");
            return;
        }

        players.Add(battlePlayer);
        DontDestroyOnLoad(playerInput.gameObject);

        // needed for NetworkVariables to work without raising warnings, but theoretically not needed for singleplayer
        // battlePlayer.GetComponent<NetworkObject>().Spawn();

        Debug.Log(battlePlayer.gameObject+" joined");
    }

    private void OnPlayerLeft(PlayerInput playerInput) {
        players.Remove(playerInput.GetComponent<BattlePlayer>());
    }

    public void EnableJoining() {
        playerInputManager.EnableJoining();
    }

    public void DisableJoining() {
        playerInputManager.DisableJoining();
    }

    public void DisconnectAllPlayers() {
        foreach (var player in players) {
            Destroy(player.gameObject);
        }
    }

    public void EnableBattleInputs() {
        foreach (BattlePlayer player in players) {
            player.EnableBattleInputs();
        }
    }

    public void DisableBattleInputs() {
        foreach (BattlePlayer player in players) {
            player.DisableBattleInputs();
        }
    }
}