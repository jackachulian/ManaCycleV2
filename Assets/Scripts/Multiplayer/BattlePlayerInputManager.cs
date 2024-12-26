using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the connecting of seperate input devices as separate players in local multiplayer.
/// </summary>
public class BattlePlayerInputManager : MonoBehaviour {
    public InputActionAsset actions;

    public static BattlePlayerInputManager instance {get; set;}

    private PlayerInputManager playerInputManager;

    private List<BattlePlayer> players;

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("More than one BattlePlayerInputManager was loaded!");
        }

        instance = this;

        players = new List<BattlePlayer>();

        playerInputManager = GetComponent<PlayerInputManager>();

        // enable all actions - the action asses tend to remain disabled when they shouldnt be
        actions.Enable();
        foreach (var map in actions.actionMaps) {
            map.Enable();
        }
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
        battlePlayer.GetComponent<NetworkObject>().Spawn();

        Debug.Log(battlePlayer.gameObject+" joined");
    }

    private void OnPlayerLeft(PlayerInput playerInput) {
        players.Remove(playerInput.GetComponent<BattlePlayer>());
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