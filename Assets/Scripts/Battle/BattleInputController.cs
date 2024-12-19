using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles a PlayerInput's messages for the battle scene, controlling the board that is assigned.
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    public Board board;

    /// <summary>
    /// Used to compare with the last input update to compare magnitudes and input when crossing a threshold.
    /// </summary>
    private float prevAnalogMoveMagnitude;

    enum AnalogMoveDirection {
        NONE,
        UP,
        RIGHT,
        LEFT,
        DOWN
    }
    private AnalogMoveDirection analogDirectionPressed = AnalogMoveDirection.NONE;

    public void OnAnalogMove(InputValue value) {
        Vector2 inputVector = value.Get<Vector2>();
        AnalogMoveDirection newDirection;

        float magnitude = inputVector.magnitude;

        // Number that determines the threshold between a direction pressed and not pressed
        float deadzone = 0.4f;

        if (magnitude < deadzone) {
            newDirection = AnalogMoveDirection.NONE;
        } else {
            float angle = Vector2.SignedAngle(Vector2.up, inputVector);

            // Up
            if (angle > -45 && angle <= 45) {
                newDirection = AnalogMoveDirection.UP;
            }
                
            // Right
            else if (angle > 45 && angle <= 135) {
                newDirection = AnalogMoveDirection.RIGHT;
            }

            // Left
            else if (angle > -135 && angle <= -45) {
                newDirection = AnalogMoveDirection.LEFT;
            }

            // Down
            else {
                newDirection = AnalogMoveDirection.DOWN;
            }
        }

        if (analogDirectionPressed != newDirection) {
            analogDirectionPressed = newDirection;
            OnAnalogMoveDirectionChanged();
        }
    }

    /// <summary>
    /// Called when the analog move input direction changes. Used for joysticks
    /// </summary>
    public void OnAnalogMoveDirectionChanged() {
        // up: ability? or hold current piece? idk yet
        if (analogDirectionPressed == AnalogMoveDirection.UP) {
            // TODO: activate ability
        }

        // right: move piece right
        else if (analogDirectionPressed == AnalogMoveDirection.RIGHT) {
            board.TryMovePiece(Vector2Int.right);
        }

        // left: move piece left
        else if (analogDirectionPressed == AnalogMoveDirection.LEFT) {
            board.TryMovePiece(Vector2Int.left);
        }

        // down: quickfall
        else if (analogDirectionPressed == AnalogMoveDirection.DOWN) {
            // TODO: set quickfall to on, make sure it is set to off when down input stops
        }
    }

    public void OnMoveLeft() {
        board.TryMovePiece(Vector2Int.left);
    }

    public void OnMoveRight() {
        board.TryMovePiece(Vector2Int.right);
    }

    public void OnQuickfall() {
        // TODO: set quickfall to true when pressed, false when unpressed
    }
}