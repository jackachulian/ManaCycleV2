using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace StoryMode.Overworld
{
    public class OverworldPlayerInteracter : MonoBehaviour
    {
        [SerializeField] private InteractionManager interactionManager;

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) {
                if (interactionManager.nearest) interactionManager.nearest.OnInteract(this);
            }
        }
    }
}