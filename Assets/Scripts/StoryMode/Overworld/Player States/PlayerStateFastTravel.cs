using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace StoryMode.Overworld
{
    // Fast-traveling player movement
    public class PlayerStateFastTravel : PlayerStateBase
    {
        private OverworldInteractable[] travelPoints;
        private OverworldInteractable selectedPoint;
        [SerializeField] private float smoothTime = 0.2f;
        private Vector3 refVel = new();

        void Awake()
        {
            // get all fast travel points in scene
            travelPoints = FindObjectsByType<OverworldInteractable>(FindObjectsSortMode.None).Where(o => o.isFastTravelPoint).ToArray();
        }

        void OnEnable()
        {
            selectedPoint = FindClosestPoint(transform.position);
        }

        void Update()
        {
            transform.position = Vector3.SmoothDamp(transform.position, selectedPoint.transform.position, ref refVel, smoothTime);
        }

        OverworldInteractable FindClosestPoint(Vector3 position)
        {
            OverworldInteractable closest = travelPoints[0];
            foreach (OverworldInteractable oi in travelPoints)
            {
                if (Vector3.Distance(position, oi.transform.position) < Vector3.Distance(position, closest.transform.position))
                    closest = oi;
            }
            return closest;
        }

        public override void OnMove(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !enabled) return;
            OverworldInteractable newPoint = selectedPoint.GetAdjacentInDir(ctx.ReadValue<Vector2>(), 90f);
            if (newPoint) selectedPoint = newPoint;
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
            // Do nothing
        }

        public override void OnInteract(InputAction.CallbackContext ctx)
        {
            // Do nothing
        }

        public override void OnFastTravel(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
        }
    }
}