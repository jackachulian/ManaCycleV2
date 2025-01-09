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
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;


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
            onStateChanged.Invoke();
        }}
    public event Action onStateChanged;


    private Battler selectedBattler;


    // Player currently controlling this board as set by the CharSelectManager, or null if no player.
    private Player player;

    public CharSelectorUI ui {get; private set;}

    void Awake() {
        ui = GetComponent<CharSelectorUI>();
    }

    /// <summary>
    /// Assign a player to control this char selector.
    /// </summary>
    public void AssignPlayer(Player player) {
        this.player = player;
        player.charSelectInputHandler.SetCharSelector(this);
        ui.ShowSelectText();
        cursor.SetPlayer(player);
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

    /// <summary>
    /// When the submit action is pressed by the player
    /// Use the cursor if it's unlocked.
    /// </summary>
    public void Submit() {
        if (!cursor.locked) cursor.Submit();
    }

    /// <summary>
    /// Called when the player pressed a character with their cursor, confirming their choice. Open the options menu next.
    /// </summary>
    public void ConfirmCharacterChoice(Battler battler) {
        Assert.AreEqual(state, CharSelectorState.ChoosingCharacter);

        selectedBattler = battler;
        ui.SetBattler(battler);
        OpenOptions();
    }

    public void OpenOptions() {
        Assert.AreEqual(state, CharSelectorState.ChoosingCharacter);

        state = CharSelectorState.Options;
        LockCursor();
        ui.OpenOptions(player.multiplayerEventSystem);
    }

    /// <summary>
    /// Called after player pressed submit on the options menu and confirms their selected battle settings
    /// </summary>
    public void ConfirmOptions() {
        Assert.AreEqual(state, CharSelectorState.Options);

        ui.CloseOptions(player.multiplayerEventSystem);
        UnlockCursor();
        state = CharSelectorState.Ready;
        ui.ShowReadyVisual();
    }

    /// <summary>
    /// Called when player presses cancel on either the options menu or the ready state, taking them back to the character selection cursor.
    /// </summary>
    public void CancelOptionsOrReady() {
        Assert.AreEqual(state, CharSelectorState.Ready);

        state = CharSelectorState.ChoosingCharacter;
        ui.HideReadyVisual();
    }

    public void SetDisplayedBattler(Battler battler) {
        ui.SetBattler(battler);
    }

    public bool IsPlayerConnected() {
        return player != null;
    }

    public bool IsReady() {
        return state == CharSelectorState.Ready;
    }
}