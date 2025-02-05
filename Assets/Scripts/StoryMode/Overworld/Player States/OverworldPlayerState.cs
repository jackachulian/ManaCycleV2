using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    public abstract class OverworldPlayerState : MonoBehaviour
    {
        public virtual void OnStateEntered() {}
        public virtual void OnStateExited() {}

        public virtual void OnMove(InputAction.CallbackContext ctx) {}
        public virtual void OnJump(InputAction.CallbackContext ctx) {}
        public virtual void OnInteract(InputAction.CallbackContext ctx) {}
        public virtual void OnFastTravel(InputAction.CallbackContext ctx) {}
    }
}