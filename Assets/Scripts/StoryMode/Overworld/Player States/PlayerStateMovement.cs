using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace StoryMode.Overworld
{
    // Regular player movement (walking)
    public class PlayerStateMovement : PlayerStateBase
    {
        [Header("Component References")]
        [SerializeField] private OverworldPlayer player;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private OverworldPlayerInteracter playerInteracter;

        [Header("Flags")]
        [SerializeField] private bool jumpingEnabled = true;

        [Header("Player Movement Parameters")]
        [Tooltip("Movement Speed Multiplier.")]
        [SerializeField] private float moveSpeed = 5f;
        [Tooltip("Gravity Scale.")]
        [SerializeField] private float gravity = 1f;
        [SerializeField] private float maxFallVel = 1f;
        [SerializeField] private float jumpSpeed = 0.5f;
        [Tooltip("Amount y-vel is reduced by when jump is released.")]
        [SerializeField] private float jumpDamping = 0.9f;

        private Vector3 moveDir;
        private Vector3 currentVel;

        private bool jumpPressed = false;

        // layer 6 is nocollide layer
        readonly int layerMask = 1 << 6;

        // FixedUpdate is used to modify player physics to due framerate independance
        void FixedUpdate()
        {
            Vector3 pos = rigidBody.position;
            // Set lateral velocity
            currentVel.x = moveSpeed * moveDir.x;
            currentVel.z = moveSpeed * moveDir.z;

            RaycastHit hit = new();
            bool grounded = IsGrounded(out hit);
            // Set falling velocity
            if (!grounded)
                currentVel.y = Mathf.Max(currentVel.y - gravity, -maxFallVel);
            else 
            {
                currentVel.y = 0;
                pos.y = hit.point.y + boxCollider.size.y * 0.5f;
            }

            // TODO add drop shadow visual
            if (jumpPressed && grounded)
            {
                currentVel.y = jumpSpeed;
            }

            if (!jumpPressed && currentVel.y > 0)
            {
                currentVel.y *= jumpDamping;
            }

            // apply velocity
            // FixedUpdate runs at a 50 times per second, unrelated to framerate. dt need not apply
            pos += currentVel;
            rigidBody.position = pos;
        }

        // Update is used to modify player visuals
        void Update()
        {
            if (moveDir != Vector3.zero)
            {
                modelTransform.forward = moveDir;
            }
        }

        public override void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            moveDir = new Vector3(v.x, 0, v.y);
            player.animator.SetBool("running", moveDir.magnitude > 0);
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
            jumpPressed = ctx.performed && jumpingEnabled; 
        }

        public override void OnInteract(InputAction.CallbackContext ctx)
        {
            playerInteracter.OnInteract(ctx);
        }

        private bool IsGrounded(out RaycastHit hit)
        {
            return Physics.Raycast(
                transform.position, 
                transform.TransformDirection(Vector3.down), 
                out hit, 
                1.05f, 
                ~layerMask, 
                QueryTriggerInteraction.UseGlobal
            );
        }
        
        void OnDisable()
        {
            moveDir = Vector3.zero;
            jumpPressed = false;
        }

        public override void OnFastTravel(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.FastTravel);
        }
    }
}
