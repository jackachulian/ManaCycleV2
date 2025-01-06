using UnityEngine;
using UnityEngine.InputSystem;
using Battle;
using System.Collections.Generic;

// ties multiple systems in the CSS together
public class CharSelectMenuController : MonoBehaviour
{
    [SerializeField] private CharPortriat[] portriats;
    [SerializeField] private List<PlayerCursorController> cursors = new();

    public void PlayerJoinHandler(PlayerInput playerInput)
    {
        var cursor = playerInput.gameObject.GetComponent<PlayerCursorController>();
        cursor.cursorInteracter.CursorSubmit += CursorSubmitHandler;
        cursor.cursorInteracter.CursorHover += CursorHoverHandler;
        cursor.cursorInteracter.CursorReturn += CursorReturnHandler;
        cursors.Add(cursor);
    }

    public void PlayerLeaveHandler(PlayerInput playerInput)
    {
        var cursor = playerInput.gameObject.GetComponent<PlayerCursorController>();
        cursor.cursorInteracter.CursorSubmit -= CursorSubmitHandler;
        cursor.cursorInteracter.CursorHover -= CursorHoverHandler;
        cursor.cursorInteracter.CursorReturn -= CursorReturnHandler;
        cursors.Remove(cursor);
    }

    // character button is selected
    public void CursorSubmitHandler(int playerNum, GameObject interacted)
    {
        Battler b = interacted.GetComponent<CharButton>().battler;
        portriats[playerNum].SetBattler(b);
        portriats[playerNum].SetLocked(true);
        // freeze cursor
        cursors[playerNum].SetEnabled(false);
    }

    // character button is hovered
    public void CursorHoverHandler(int playerNum, GameObject interacted)
    {
        Battler b = interacted.GetComponent<CharButton>().battler;
        portriats[playerNum].SetBattler(b);
        portriats[playerNum].SetLocked(false);
    }

    public void CursorReturnHandler(int playerNum)
    {
        cursors[playerNum].SetEnabled(true);
        portriats[playerNum].SetLocked(false);
    }
}
