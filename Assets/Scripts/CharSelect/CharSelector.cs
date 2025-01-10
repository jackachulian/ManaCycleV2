using System;
using Battle;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Handles a char selector, locked status, etc. 
/// Visuals should be handled by CharSelectorPortrait on this same object.
/// </summary>
public class CharSelector : MonoBehaviour {
    // TODO: move locked in status and other things that need a concrete value here.
    // Should also either move the options UI here or make a seperate behaviour for it.
    // The cursor that belongs to this charselector. There is one attached to each charselector.
    [SerializeField] private Cursor cursor;


    public enum CharSelectorState {
        ChoosingCharacter,
        Options,
        Ready
    }
    private CharSelectorState _state;
    public CharSelectorState state {
        get {return _state;} 
        private set {
            _state = value;
            player.charSelectorState.Value = _state;
            onStateChanged?.Invoke();
        }}
    public event Action onStateChanged;


    public Battler selectedBattler {get; private set;}
    public int selectedBattlerIndex;


    // Player currently controlling this board as set by the CharSelectManager, or null if no player.
    private Player player;

    public CharSelectorUI ui {get; private set;}

    void Awake() {
        ui = GetComponent<CharSelectorUI>();
        cursor.Hide();
    }

    /// <summary>
    /// Assign a player to control this char selector.
    /// Null to have no player assigned to this selector
    /// </summary>
    public void AssignPlayer(Player player) {
        this.player = player;
        if (player) {
            if (!ui) {
                Debug.LogError("UI not found");
                return;
            }
            ui.ShowSelectText();
            ui.UpdatePlayerData(player);
            cursor.Show();
            cursor.SetPlayer(player);
        } else {
            cursor.Hide();
            ui.ShowUnconnectedText();
        }
    }

    /// <summary>
    /// Called by a Player when it disconnects from a char selector.
    /// </summary>
    public void UnassignPlayer() {
        BackToCharacterChoice();
        ui.ShowDisconnectedText();
        ui.UpdatePlayerData(null);
    }

    /// <summary>
    /// Unlock the cursor, hide the options menu, and allow player to choose a character with the on-screen cursor
    /// </summary>
    public void UnlockCursor() {
        cursor.SetLocked(false);
    }

    public void LockCursor() {
        cursor.SetLocked(true);
    }

    public void MoveCursor(Vector2 inputVector)
    {
        cursor.Move(inputVector);
    }

    public void SetCursorPosition(Vector2 position)
    {
        cursor.SetPosition(position);
    }

    /// <summary>
    /// When the submit action is pressed by the player
    /// Use the cursor if it's unlocked.
    /// </summary>
    public void Submit() {
        // Ready up when pressing submit while the options menu is open
        if (state == CharSelectorState.Options) {
            ConfirmOptions();
        } 
        // otherwise, click with the cursor if it is not locked due to already being ready
        else if (!cursor.locked) {
            cursor.Submit();
        }
    }

    /// <summary>
    /// Called when the player pressed a character with their cursor, confirming their choice. Open the options menu next.
    /// </summary>
    public void ConfirmCharacterChoice(Battler battler) {
        // don't do anything when not choosing a character when this is pressed
        if (state != CharSelectorState.ChoosingCharacter) return;
        
        Debug.Log("Character choice confirmed");

        selectedBattler = battler;
        player.selectedBattler.Value = selectedBattlerIndex;
        ui.SetBattler(battler);
        OpenOptions();
    }

    public void OpenOptions() {
        Debug.Log("Options opened");
        Assert.AreEqual(state, CharSelectorState.ChoosingCharacter);

        state = CharSelectorState.Options;
        ui.characterChoiceConfirmed = true;
        LockCursor();
        ui.OpenOptions(player.multiplayerEventSystem);
        ui.SetLockedVisual();
        ui.ShowReadyVisual();
    }

    /// <summary>
    /// Called after player pressed submit on the options menu and confirms their selected battle settings
    /// </summary>
    public void ConfirmOptions() {
        Debug.Log("Options confirmed and closed");
        Assert.AreEqual(state, CharSelectorState.Options);


        ui.CloseOptions(player.multiplayerEventSystem);
        player.selectedBattler.Value = selectedBattlerIndex;
        state = CharSelectorState.Ready;
        ui.characterChoiceConfirmed = true;
        LockCursor();
        ui.SetLockedVisual();
        ui.ShowReadyVisual();
    }

    /// <summary>
    /// Called when player presses cancel on either the options menu or the ready state, taking them back to the character selection cursor.
    /// </summary>
    public void Cancel() {
        BackToCharacterChoice();
    }

    public void BackToCharacterChoice() {
        // don't do anything when already choosing a character
        // except TODO: maybe a player can hold cancel to go back to the previous menu.
        if (state == CharSelectorState.ChoosingCharacter) return;

        Debug.Log("Going back to choosing character");

        state = CharSelectorState.ChoosingCharacter;
        ui.characterChoiceConfirmed = false;
        ui.CloseOptions(player.multiplayerEventSystem);
        UnlockCursor();
        ui.HideReadyVisual();
        ui.SetLockedVisual();
    }

    public void SetDisplayedBattler(Battler battler) {
        ui.SetBattler(battler);
    }

    public bool IsPlayerConnected() {
        return player != null;
    }

    public bool IsCharacterDecided() {
        return state != CharSelectorState.ChoosingCharacter;
    }

    public bool IsOptionsDecided() {
        return state == CharSelectorState.Ready;
    }

    public bool IsReady() {
        return state == CharSelectorState.Ready;
    }
}