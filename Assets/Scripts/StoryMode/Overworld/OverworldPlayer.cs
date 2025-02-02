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
        public static OverworldPlayer Instance {get; private set;}

        [SerializeField] private Animator _animator;
        public Animator animator => _animator;

        /// <summary>
        /// All states. Index corresponds to the PlayerState enum.
        /// </summary>
        [SerializeField] private PlayerStateBase[] states;


        private PlayerInput _playerInput;
        public PlayerInput playerInput => _playerInput;


        public enum PlayerState
        {
            Movement,
            FastTravel,
            Convo,
            LevelDetails
        }

        public PlayerState ActiveState {get; private set;}

        void Awake()
        {
            if (Instance) {
                Debug.Log("Duplicate overworld player; destroying duplicate");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _playerInput = GetComponent<PlayerInput>();

            // FIXME this finds all states in scene not in object...
            SetState(PlayerState.Movement);
        }

        // fast travel enabled only when held
        public void OnFastTravel(InputAction.CallbackContext ctx)
        {
            
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
            Debug.Log("Changing to state "+state);

            foreach(var s in states)
            {
                s.enabled = false;
            }
            states[(int)state].enabled = true;
            ActiveState = state;
        }
        
    }
}