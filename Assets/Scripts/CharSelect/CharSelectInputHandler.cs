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
    public CharSelector charSelector;

    /// <summary>
    /// Set the char selector that this player should now control. Called from CharSelectManager when a player is added.
    /// </summary>
    public void SetCharSelector(CharSelector charSelector) {
        this.charSelector = charSelector;
    }

    public void OnSubmit()
    {
        if (charSelector) charSelector.Submit();
    }

    void OnCancel()
    {
        if (charSelector) charSelector.Cancel();
    }

    public void OnNavigate(InputValue value)
    {
        if (!charSelector) return;
        Vector2 inputVector = value.Get<Vector2>();
        charSelector.MoveCursor(inputVector);
    }
}

