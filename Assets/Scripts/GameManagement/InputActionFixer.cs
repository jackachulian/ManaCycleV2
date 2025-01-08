using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles local player connections for a game manager.
/// </summary>
public class InputActionFixer : MonoBehaviour {
    public InputActionAsset inputActions;

    void Awake() {
        inputActions.Enable();
    }
}