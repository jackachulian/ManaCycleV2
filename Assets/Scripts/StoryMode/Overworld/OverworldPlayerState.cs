using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    public abstract class OverworldPlayerState : MonoBehaviour
    {
        public abstract void OnMove(InputAction.CallbackContext ctx);
        public abstract void OnJump(InputAction.CallbackContext ctx);
        public abstract void OnInteract(InputAction.CallbackContext ctx);
    }
    
}