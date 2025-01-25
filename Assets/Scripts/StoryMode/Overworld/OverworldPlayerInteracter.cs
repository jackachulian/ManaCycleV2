using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace StoryMode.Overworld
{
    public class OverworldPlayerInteracter : MonoBehaviour
    {
        [SerializeField] private InteractionManager interactionManager;
        private OverworldInteractable? currentInteractable = null;

        void Start()
        {
            interactionManager.InteractableChangeNotifier += SetInteractable;
        }

        private void SetInteractable(OverworldInteractable interactable)
        {
            currentInteractable = interactable;
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && currentInteractable) currentInteractable.OnInteract();
        }
    }
}