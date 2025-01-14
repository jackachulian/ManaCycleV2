using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

/// <summary>
/// Handles inputs in the character select screen.
/// Should only be enabled on players during the character select scene.
/// </summary>
public class CharSelectInputHandler : NetworkBehaviour
{
    /// <summary>
    /// The character selector this player is currently controlling.
    /// Contains the character selector box, portrait, cursor and options menu currently being controlled by this player
    /// </summary>
    public CharSelector charSelector {get; private set;}

    /// <summary>
    /// Cached player component
    /// </summary>
    private Player player;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Set the char selector that this player should now control. Called from CharSelectManager when a player is added.
    /// </summary>
    public void SetCharSelector(CharSelector charSelector) {
        this.charSelector = charSelector;
    }

    public void OnSubmit()
    {
        if (!IsOwner) {
            Debug.LogError("Cannot submit on a charselector you do not own!");
            return;
        }

        if (!charSelector) {
            // Debug.LogError("No charselector assigned");
            return;
        }

        // Confirm options if options is open
        if (player.characterChosen.Value) {
            player.optionsChosen.Value = true;
        } 

        // Click with the cursor if still choosing character
        else {
            charSelector.ui.cursor.interactor.Submit();
        }
    }

    public void OnCancel()
    {
        if (!IsOwner) {
            Debug.LogError("Cannot cancel on a charselector you do not own!");
            return;
        }

        player.optionsChosen.Value = false;
        player.characterChosen.Value = false;
    }

    public void OnNavigate(InputValue value)
    {
        if (!charSelector) return;
        Vector2 inputVector = value.Get<Vector2>();
        charSelector.ui.cursor.Move(inputVector);
    }

    // controlling cursor with actual mouse is buggy, disabling it for now, may re-enable later
    // public void OnPoint(InputValue value) {
    //     if (!charSelector) return;
    //     Vector2 screenPosition = value.Get<Vector2>();
    //     charSelector.SetCursorPosition(screenPosition);
    // }
}

