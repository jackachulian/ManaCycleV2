using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace StoryMode.Overworld
{
    public class OverworldPlayerMovement : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private BoxCollider boxCollider;

        [Header("Flags")]
        [SerializeField] private bool jumpingEnabled = true;

        [Header("Player Movement Parameters")]
        [Tooltip("Movement Speed Multiplier.")]
        [SerializeField] private float moveSpeed = 1f;
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
        int layerMask = 1 << 6;

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

            if (jumpPressed && grounded)
            {
                currentVel.y = jumpSpeed;
            }

            if (!jumpPressed && currentVel.y > 0)
            {
                currentVel.y *= jumpDamping;
            }

            // apply velocity
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

        public void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            moveDir = new Vector3(v.x, 0, v.y);
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            jumpPressed = ctx.performed && jumpingEnabled; 
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
    }
}
