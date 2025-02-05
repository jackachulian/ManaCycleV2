using StoryMode.ConvoSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    public class PlayerStateConvo : OverworldPlayerState
    {
        public override void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && ConvoManager.currentConvoUI)
                ConvoManager.currentConvoUI.HandleForwardInput();
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && ConvoManager.currentConvoUI)
                ConvoManager.currentConvoUI.HandleBackwardInput();
        }
    }
}