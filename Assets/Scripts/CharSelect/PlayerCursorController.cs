using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursorController : MonoBehaviour
{
    public PlayerCursorMovement cursorMovement;
    public UIInteracter cursorInteracter;

    // true when character and options are fully submitted
    public bool locked = false;

    public void SetEnabled(bool enable)
    {
        cursorMovement.enabled = enable;
        cursorInteracter.enabled = enable;
    }
}

