using UnityEngine;
using UnityEngine.InputSystem;

namespace StoryMode.Overworld
{
    /// <summary>
    /// different player "states" (walking, fast travel, convo) are implemented as different components.
    /// This script contains the logic that enables and disables each component
    /// </summary>
    public class OverworldPlayer : MonoBehaviour
    {
        private OverworldPlayerState[] states;

        public enum PlayerState
        {
            Normal,
            Fast,
            Convo
        }

        public PlayerState ActiveState {get; private set;} = PlayerState.Normal;

        void Awake()
        {
            ActiveState = PlayerState.Normal;
            // FIXME this finds all states in scene not in object...
            states = FindObjectsByType<OverworldPlayerState>(FindObjectsSortMode.None);
        }

        // fast travel enabled only when held
        public void OnFastTravel(InputAction.CallbackContext ctx)
        {
            SetState(ctx.performed ? PlayerState.Fast : PlayerState.Normal);
        }

        // re-route inputs to active state to avoid interference and only serialize inputs once
        public void OnMove(InputAction.CallbackContext ctx)
        {
            states[(int)ActiveState].OnMove(ctx);
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            states[(int)ActiveState].OnJump(ctx);
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            states[(int)ActiveState].OnInteract(ctx);
        }

        public void SetState(PlayerState state)
        {
            foreach(var s in states)
            {
                s.enabled = false;
            }
            states[(int)state].enabled = true;
            ActiveState = state;
        }
        
    }
}