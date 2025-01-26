using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    public abstract class PlayerStateBase : MonoBehaviour
    {
        public abstract void OnMove(InputAction.CallbackContext ctx);
        public abstract void OnJump(InputAction.CallbackContext ctx);
        public abstract void OnInteract(InputAction.CallbackContext ctx);
    }
    
}