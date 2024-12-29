using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the connecting of seperate input devices as separate players in local multiplayer.
/// </summary>
public class BattlePlayerInputManager : MonoBehaviour {
    [SerializeField] public BattleLobbyManager battleLobbyManager;

    private PlayerInputManager playerInputManager;

    private List<BattlePlayer> players;

    private void Awake() {
        players = new List<BattlePlayer>();

        playerInputManager = GetComponent<PlayerInputManager>();

        if (battleLobbyManager.battlePlayerInputManager != null) {
            Debug.LogWarning("Duplicate BattlePlayerInputManager! Destroying the old one.");
            Destroy(battleLobbyManager.battlePlayerInputManager.gameObject);
        }

        battleLobbyManager.battlePlayerInputManager = this;

        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft -= OnPlayerJoined;

        DisableJoining();
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void OnPlayerJoined(PlayerInput playerInput) {
        BattlePlayer battlePlayer = playerInput.gameObject.GetComponent<BattlePlayer>();
        if (!battlePlayer) {
            Debug.LogError("No battle player on spawned player");
            return;
        }

        players.Add(battlePlayer);
        DontDestroyOnLoad(playerInput.gameObject);

        // Assign a local player id based on the current length of the players list
        battlePlayer.localPlayerId = (ulong)(players.Count - 1);

        // Character select network behaviour menu needs to know this player joined so it knows when to check if all players are ready
        // var charSelect = battleLobbyManager.battleSetupManager.characterSelectMenu.characterSelectNetworkBehaviour;
        // charSelect.OnPlayerJoined(battlePlayer.GetId());

        // needed for NetworkVariables to work without raising warnings, but theoretically not needed for singleplayer
        // battlePlayer.GetComponent<NetworkObject>().Spawn();

        Debug.Log(battlePlayer.gameObject+" local player joined");
    }

    private void OnPlayerLeft(PlayerInput playerInput) {
        BattlePlayer battlePlayer = playerInput.GetComponent<BattlePlayer>();

        players.Remove(battlePlayer);

        battleLobbyManager.battleSetupManager.characterSelectMenu.characterSelectNetworkBehaviour.OnPlayerLeft(battlePlayer.GetId());
    }

    public void EnableJoining() {
        Debug.Log("Player joining enabled on player input manager");
        playerInputManager.EnableJoining();
    }

    public void DisableJoining() {
        Debug.Log("Player joining disabled on player input manager");
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