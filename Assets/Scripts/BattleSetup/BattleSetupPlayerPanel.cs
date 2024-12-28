using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// A BattleSetupPlayerPanel controls a single player and the panel in the BattleSetup for choosing character and other settings.
/// </summary>
public class BattleSetupPlayerPanel : MonoBehaviour
{
    /// <summary>
    /// checkbox for when the player is ready
    /// (PROBABLY TEMPORARY, will be replaced with selecting a character, RNG and drop speed.)
    /// </summary>
    [SerializeField] private Toggle toggle;

    /// <summary>
    /// The BattlePlayer currently assigned to this board, according to the RPCs sent by CharacterSelectNetworkBehaviour.
    /// </summary>
    private BattlePlayer battlePlayer;

    public void InitializeBattleSetup(CharacterSelectMenu characterSelectMenu) {
        
    }

    /// <summary>
    /// Assign a player to this setup panel.
    /// If null is assigned, no player will control this panel.
    /// </summary>
    /// <param name="player">the BattlePlayer player object</param>
    public void AssignPlayer(BattlePlayer player) {
        
        // If new player is the same player or still null, return
        if (battlePlayer == player) return;

        // Unregister callbacks if there is already a battleplayer
        if (battlePlayer) {
            Debug.Log("Unassigning client id "+battlePlayer.OwnerClientId+" from setup panel object "+this);

            toggle.interactable = false;
            battlePlayer.ready.OnValueChanged -= OnReadyChanged;

            // only stop listening for UI callbacks if the player actually controls the toggle
            if (battlePlayer.IsOwner) toggle.onValueChanged.RemoveListener(OnReadyToggleChanged);
        }

        battlePlayer = player;

        // Register callbacks if new player is not null
        if (battlePlayer) {
            Debug.Log("Assigning player object of client id "+battlePlayer.OwnerClientId+" to setup panel object "+this);

            toggle.interactable = battlePlayer.IsOwner;
            battlePlayer.ready.OnValueChanged += OnReadyChanged;

            // only listen for ui callbacks if the player actually controls the toggle
            if (battlePlayer.IsOwner) toggle.onValueChanged.AddListener(OnReadyToggleChanged);
        }

        
    }

    // public override void OnNetworkSpawn() {
    //     ready.OnValueChanged += OnReadyChanged;
    //     toggle.onValueChanged.AddListener(OnReadyToggleChanged);
    // }

    // public override void OnNetworkDespawn() {
    //     ready.OnValueChanged -= OnReadyChanged;
    //     toggle.onValueChanged.RemoveListener(OnReadyToggleChanged);
    // }


    /// <summary>
    /// For when the ready toggle is clicked by the user.
    /// Modify the NetworkVariable on the assigned player object.
    /// (This will only work if the client owns the assigned player)
    /// </summary>
    public void OnReadyToggleChanged(bool value) {
        if (battlePlayer) {
            battlePlayer.ready.Value = value;
        } else {
            Debug.LogError("Trying to change ready status, but no player connected to this board!");
        }
    }

    /// <summary>
    /// For when the ready networkvariable is changed by this or another client. Update the toggle to reflect the new value
    /// </summary>
    public void OnReadyChanged(bool previous, bool current) {
        // Debug.Log(this+" ready was changed from "+previous+" to "+current);
        // change value without notify (notify would call OnReadyToggleChanged which would try to re-update the network variable!)
        toggle.SetIsOnWithoutNotify(current);
    }
}
