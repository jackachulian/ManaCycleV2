using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Represents a cursor that can be controlled using analog move inputs and used to click UI elements.
/// </summary>
public class Cursor : MonoBehaviour
{
    public event Action<CharButton> onCharButtonHovered;

    [SerializeField] private Image cursorImage;
    [SerializeField] private TMP_Text playerNumberText;

    private CursorMovement cursorMovement;
    public CursorUIInteractor interactor {get; private set;}
    public Animator animator {get; private set;}

    public bool locked = false;

    void Awake() {
        cursorMovement = GetComponent<CursorMovement>();
        interactor = GetComponent<CursorUIInteractor>();
        animator = GetComponent<Animator>();
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

    public void SetPlayer(Player player, Color color, int playerNumber) {
        interactor.SetPlayer(player);
        cursorImage.color = color;
        playerNumberText.text = "P"+playerNumber;
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

