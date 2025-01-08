using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles inputs in the character select screen.
/// Should only be enabled on players during the character select scene.
/// </summary>
public class CharSelectInputHandler : MonoBehaviour
{
    /// <summary>
    /// The character selector this player is currently controlling.
    /// Contains the character selector box, portrait, cursor and options menu currently being controlled by this player
    /// </summary>
    public CharSelector charSelector {get; private set;}

    /// <summary>
    /// Set the char selector that this player should now control. Called from CharSelectManager when a player is added.
    /// </summary>
    public void SetCharSelector(CharSelector charSelector) {
        this.charSelector = charSelector;
    }

    public void OnSubmit()
    {
        charSelector.cursor.Submit();
    }

    void OnCancel()
    {
        // if (value.isPressed) OnCursorReturn?.Invoke(playerNum);
    }

    public void OnNavigate(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        charSelector.cursor.Move(inputVector);
    }
}

