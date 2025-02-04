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

        /// <summary>
        /// All states. Index corresponds to the PlayerState enum.
        /// </summary>
        [SerializeField] private PlayerStateBase[] states;


        [SerializeField] private GameObject _modelObject;
        public GameObject modelObject => _modelObject;
        public Transform modelTransform {get; private set;}
        public Animator modelAnimator {get; private set;}


        private PlayerInput _playerInput;
        public PlayerInput playerInput => _playerInput;


        [SerializeField] private InteractionManager _interactionManager;
        public InteractionManager interactionManager => _interactionManager;

        public enum PlayerState
        {
            Movement,
            FastTravel,
            Convo,
            Menu,
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

            foreach (var state in states) {
                state.enabled = false;
            }

            SetPlayerModel(modelObject);
            SetState(PlayerState.Movement);
        }

        // fast travel enabled only when held
        public void OnFastTravel(InputAction.CallbackContext ctx)
        {
            states[(int)ActiveState].OnFastTravel(ctx);
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
            if (ActiveState == state && states[(int)ActiveState].enabled) return;

            Debug.Log("Changing to state "+state);

            var prevStateBehavior = states[(int)ActiveState];
            if (prevStateBehavior && prevStateBehavior.enabled) {
                prevStateBehavior.OnStateExited();
                prevStateBehavior.enabled = false;
            }

            ActiveState = state;
            var newStateBehavior = states[(int)ActiveState];
            if (newStateBehavior) {
                newStateBehavior.enabled = true;
                newStateBehavior.OnStateEntered();
            }
        }
        
        public void SetPlayerModel(GameObject modelObject) {
            this._modelObject = modelObject;
            modelTransform = modelObject.transform;
            modelAnimator = modelObject.GetComponent<Animator>();
        }
    }
}