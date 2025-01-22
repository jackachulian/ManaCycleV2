using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Handles a char selector, locked status, etc. 
/// Visuals should be handled by CharSelectorPortrait on this same object.
/// </summary>
public class CharSelector : MonoBehaviour {
    public Player player {get; private set;}


    public CharSelectorUI ui {get; private set;}

    void Awake() {
        ui = GetComponent<CharSelectorUI>();
    }

    /// <summary>
    /// Call this when a player, local or remote, has been assigned to control this charselector.
    /// </summary>
    public void AssignPlayer(Player player) {
        // if a player was already assigned, make sure to do any unassign logic needed here!

        this.player = player;
        Debug.Log("Assigned "+player+" to char selector "+this);
        player.OnCharacterChosenChanged(true, player.characterChosen.Value);
        GetComponent<CharSelectorUI>().OnAssignedPlayer();
    }

    public void ShowSelector()
    {
        gameObject.SetActive(true);
    }

    public void HideSelector()
    {
        gameObject.SetActive(false);
    }
}