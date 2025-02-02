using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    public abstract class PlayerStateBase : MonoBehaviour
    {
        public virtual void OnMove(InputAction.CallbackContext ctx) {}
        public virtual void OnJump(InputAction.CallbackContext ctx) {}
        public virtual void OnInteract(InputAction.CallbackContext ctx) {}
        public virtual void OnFastTravel(InputAction.CallbackContext ctx) {}
    }
}