using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Represents a cursor that can be controlled using analog move inputs and used to click UI elements.
/// </summary>
public class Cursor : MonoBehaviour
{
    public event Action<CharButton> onCharButtonHovered;

    private CursorMovement cursorMovement;
    public CursorUIInteractor interactor {get; private set;}

    public bool locked = false;

    void Awake() {
        cursorMovement = GetComponent<CursorMovement>();
        interactor = GetComponent<CursorUIInteractor>();
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
        cursorMovement.enabled = !locked;
        interactor.enabled = !locked;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void SetPlayer(Player player) {
        interactor.SetPlayer(player);
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

    public void SetPosition(Vector2 position) {
        if (locked) return;
        cursorMovement.SetPosition(position);
    }
}

