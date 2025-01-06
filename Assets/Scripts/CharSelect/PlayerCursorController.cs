using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursorController : MonoBehaviour
{
    public PlayerCursorMovement cursorMovement;
    public UIInteracter cursorInteracter;
        
    void Awake()
    {

    }

    public void SetEnabled(bool enable)
    {
        cursorMovement.enabled = enable;
        cursorInteracter.enabled = enable;
    }
}

