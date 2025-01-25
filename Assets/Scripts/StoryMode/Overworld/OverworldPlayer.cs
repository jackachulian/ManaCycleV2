using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    // Fast-traveling player movement
    public class OverworldPlayer : MonoBehaviour
    {
        [SerializeField] private OverworldPlayerMovement defaultMovement;
        [SerializeField] private OverworldPlayerFastTravel fastMovement;

        // fast travel enabled only when held
        public void OnFastTravel(InputAction.CallbackContext ctx)
        {
            defaultMovement.enabled = !ctx.performed;
            fastMovement.enabled = ctx.performed;
        }
    }
}