using UnityEngine;
using UnityEngine.InputSystem;
using Battle;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;

// ties multiple systems in the CSS together
public class CharSelectMenuController : MonoBehaviour
{
    [SerializeField] private CharPortriat[] portriats;
    [SerializeField] private List<PlayerCursorController> cursors = new();
    [SerializeField] private GameObject allReadyWindow;
    private bool allReady = false;

    public void PlayerJoinHandler(PlayerInput playerInput)
    {
        var cursor = playerInput.gameObject.GetComponent<PlayerCursorController>();
        cursor.cursorInteracter.Initialize();
        cursor.cursorInteracter.CursorSubmit += CursorSubmitHandler;
        cursor.cursorInteracter.CursorHover += CursorHoverHandler;
        cursor.cursorInteracter.CursorReturn += CursorReturnHandler;
        cursor.OnEmptySubmit += CursorEmptySubmitHandler;

        var p = portriats[cursor.cursorInteracter.playerNum];
        p.gameObject.SetActive(true);
        p.SetSelectText();
        p.ReadyStateChanged += OnReadyStateChanged;

        cursors.Add(cursor);
    }

    public void PlayerLeaveHandler(PlayerInput playerInput)
    {
        var cursor = playerInput.gameObject.GetComponent<PlayerCursorController>();
        cursor.cursorInteracter.CursorSubmit -= CursorSubmitHandler;
        cursor.cursorInteracter.CursorHover -= CursorHoverHandler;
        cursor.cursorInteracter.CursorReturn -= CursorReturnHandler;

        var p = portriats[cursor.cursorInteracter.playerNum];
        p.ReadyStateChanged -= OnReadyStateChanged;
        p.SetDefault();
        if (cursor.cursorInteracter.playerNum < 2) p.gameObject.SetActive(false);
        cursors.Remove(cursor);
    }

    // TODO this is slightly hacky
    public void CursorEmptySubmitHandler()
    {
        if (allReady) Debug.Log("Scene Transition Here");
    }

    // character button is selected
    public void CursorSubmitHandler(int playerNum, GameObject interacted)
    {
        Battler b = interacted.GetComponent<CharButton>().battler;
        var portrait = portriats[playerNum];
        var cursor = cursors[playerNum];

        portrait.SetBattler(b);
        portrait.SetLocked(true);
        // freeze cursor
        cursor.SetEnabled(false);
        // select options window
        portrait.OpenOptions(cursor);
    }

    // character button is hovered
    public void CursorHoverHandler(int playerNum, GameObject interacted)
    {
        Battler b = interacted.GetComponent<CharButton>().battler;
        var portrait = portriats[playerNum];
        
        portrait.SetBattler(b);
        portrait.SetLocked(false);
    }

    public void CursorReturnHandler(int playerNum)
    {
        var portrait = portriats[playerNum];
        var cursor = cursors[playerNum];

        cursor.SetEnabled(true);
        portrait.SetLocked(false);
        portrait.CloseOptions(cursor);
    }

    public void OnReadyStateChanged(bool ready)
    {
        Debug.Log("State change");
        allReadyWindow.SetActive(false);
        allReady = false;
        foreach (var portrait in portriats)
        {
            Debug.Log(portrait.ready);
            if (!portrait.ready) return;
        }

        allReady = true;
        // the following code is ran if all players have readied up
        allReadyWindow.SetActive(true);
    }

}
