using System;
using Battle;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Handles a char selector, locked status, etc. 
/// Visuals should be handled by CharSelectorPortrait on this same object.
/// </summary>
public class CharSelector : NetworkBehaviour {
    /// <summary>
    /// The index of the battler this player has selected
    /// </summary>
    public NetworkVariable<int> selectedBattlerIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// True when this player has locked in their character choice
    /// </summary>
    public NetworkVariable<bool> characterChosen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// True when this player has locked in their options choices and is now ready to start the game
    /// </summary>
    public NetworkVariable<bool> optionsChosen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public Player player {get; private set;}


    public CharSelectorUI ui {get; private set;}

    public override void OnNetworkSpawn() {
        ui = GetComponent<CharSelectorUI>();

        selectedBattlerIndex.OnValueChanged += OnSelectedBattlerIndexChanged;
        characterChosen.OnValueChanged += OnCharacterChosenChanged;
        optionsChosen.OnValueChanged += OnOptionsChosenChanged;
    }

    /// <summary>
    /// Call this when this client's local player has been assigned to control this charselector.
    /// </summary>
    public void AssignLocalPlayer(Player player) {
        // if a player was already assigned, make sure to do any unassign logic needed here!

        this.player = player;
        Debug.Log("Assigned "+player+" to char selector "+this);
        ui.OnAssignedPlayer();
    }

    /// <summary>
    /// Called when a LOCAL player pressed confirm to lock in their current choices (character or options).
    /// </summary>
    public void Submit() {
        if (characterChosen.Value) {
            optionsChosen.Value = true;
        } else if (selectedBattlerIndex.Value >= 0) {
            characterChosen.Value = true;
        }
    }

    /// <summary>
    /// Called when a LOCAL player presses the cancel button.
    /// Go back to the character select, no matter if this player is ready or still choosing options.
    /// </summary>
    public void Cancel() {
        optionsChosen.Value = false;
        characterChosen.Value = false;
    }

    public void OnSelectedBattlerIndexChanged(int previous, int current) {
        ui.UpdateSelectedBattler();
    }

    public void OnCharacterChosenChanged(bool previous, bool current) {
        ui.UpdateReadinessStatus();
    }

    public void OnOptionsChosenChanged(bool previous, bool current) {
        ui.UpdateReadinessStatus();
        ui.UpdateSelectedBattler();
    }
}