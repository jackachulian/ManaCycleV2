using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles a PlayerInput's messages for the battle scene, controlling the board that is assigned.
/// </summary>
public class BattleInputController : MonoBehaviour
{
    public Board board;

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

            if (angle > -45 && angle <= 45) {
                newDirection = AnalogMoveDirection.UP;
            }
            else if (angle > 45 && angle <= 135) {
                newDirection = AnalogMoveDirection.LEFT;
            }
            else if (angle > -135 && angle <= -45) {
                newDirection = AnalogMoveDirection.RIGHT;
            }
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
            board.pieceManager.TryMovePiece(Vector2Int.right);
        }

        // left: move piece left
        else if (analogDirectionPressed == AnalogMoveDirection.LEFT) {
            board.pieceManager.TryMovePiece(Vector2Int.left);
        }

        // down: quickfall
        else if (analogDirectionPressed == AnalogMoveDirection.DOWN) {
            // TODO: set quickfall to on, make sure it is set to off when down input stops
        }
    }

    public void OnMoveLeft() {
        board.pieceManager.TryMovePiece(Vector2Int.left);
    }

    public void OnMoveRight() {
        board.pieceManager.TryMovePiece(Vector2Int.right);
    }

    public void OnRotateCCW() {
        board.pieceManager.TryRotatePiece(-1);
    }

    public void OnRotateCW() {
        board.pieceManager.TryRotatePiece(1);
    }

    public void OnQuickfall(InputValue value) {
        float pressed = value.Get<float>();
        
        if (pressed >= 0.5) {
            board.pieceManager.SetQuickfall(true);
        } else {
            board.pieceManager.SetQuickfall(false);
        }
    }

    public void OnSpellcast() {
        board.spellcastManager.TrySpellcast();
    }
}