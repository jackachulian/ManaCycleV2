using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Represents a cursor that can be controlled using analog move inputs and used to click UI elements.
/// </summary>
public class Cursor : MonoBehaviour
{
    public event Action<CharButton> onCharButtonHovered;

    [SerializeField] private CursorMovement cursorMovement;
    [SerializeField] public CursorUIInteractor interactor;

    public bool locked {get; private set;} = false;

    public void SetLocked(bool locked)
    {
        this.locked = locked;
        cursorMovement.enabled = !locked;
        interactor.enabled = !locked;
    }

    /// <summary>
    /// Use to click whatever object is hovered, if any.
    /// </summary>
    public void Submit()
    {
        interactor.Submit();
    }

    public void Move(Vector2 inputVector)
    {
        cursorMovement.SetCursorMovement(inputVector);
    }
}

