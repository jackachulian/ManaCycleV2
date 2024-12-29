using System;
using TMPro;
using Unity.Collections;
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
    /// Displays the username in online mode.
    /// </summary>
    [SerializeField] private TMP_Text usernameText;

    /// <summary>
    /// checkbox for when the player is ready
    /// (PROBABLY TEMPORARY, will be replaced with selecting a character, RNG and drop speed.)
    /// </summary>
    [SerializeField] private Toggle readyToggle;

    /// <summary>
    /// Cached canvasgroup component.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// The BattlePlayer currently assigned to this board, according to the RPCs sent by CharacterSelectNetworkBehaviour.
    /// </summary>
    private BattlePlayer battlePlayer;

    public void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();

        // start off as mostly transparent when no player connected (may change this later)
        canvasGroup.alpha = 0.4f;

        // hide whatever placeholder username there is until a player connects to this panel
        SetUsername("");

        // toggle starts as uninteractable, will become interactable when the local client controls this panel
        readyToggle.interactable = false;
    }

    public void InitializeBattleSetup(CharacterSelectMenu characterSelectMenu) {
        // there was code here but not anymore, most code will be in OnNetworkSpawn
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
            Debug.LogWarning("Assigning a player to an already occupied panel. Previous player will now have no panel. Make sure you aren't stealing a panel from a player that needs it!");
            Debug.Log("Unassigning player with id "+battlePlayer.GetId()+" from setup panel object "+this);

            readyToggle.interactable = false;
            battlePlayer.ready.OnValueChanged -= OnReadyChanged;

            SetUsername("");

            // semi-transparent when no player connected
            canvasGroup.alpha = 0.4f;

            // only stop listening for UI callbacks if the player actually controls the toggle
            if (battlePlayer.IsOwner) readyToggle.onValueChanged.RemoveListener(OnReadyToggleChanged);
        }

        battlePlayer = player;

        // Register callbacks if new player is not null
        if (battlePlayer) {
            Debug.Log("Assigning player object of id "+battlePlayer.GetId()+" to setup panel object "+this);

            readyToggle.interactable = battlePlayer.IsOwner;
            battlePlayer.ready.OnValueChanged += OnReadyChanged;

            SetUsername(battlePlayer.username.Value.ToString());

            // fully opaque when a player is connected
            canvasGroup.alpha = 1f;

            // only listen for ui callbacks if the player actually controls the toggle
            if (battlePlayer.IsOwner) readyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
        }
    }

    /// <summary>
    /// For when a player connects and after their username is set asynchronously from their client.
    /// </summary>
    public void SetUsername(string username) {
        usernameText.text = username;
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
        readyToggle.SetIsOnWithoutNotify(current);
    }
}
