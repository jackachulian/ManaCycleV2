using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattlePlayerInputManager : PlayerInputManager {
    public new static BattlePlayerInputManager instance {get; set;}

    public List<BattlePlayer> players;

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("More than one BattlePlayerInputManager was loaded!");
        }

        instance = this;
    }

    private void Start() {
        DontDestroyOnLoad(this);
    }

    private void OnPlayerJoined(PlayerInput playerInput) {
        players.Add(playerInput.GetComponent<BattlePlayer>());
        DontDestroyOnLoad(playerInput.gameObject);
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