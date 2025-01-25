using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace StoryMode.Overworld
{
    // Fast-traveling player movement
    public class OverworldPlayerFastTravel : MonoBehaviour
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

        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !enabled) return;
            Vector2 v = ctx.ReadValue<Vector2>();
            selectedPoint = selectedPoint.GetAdjacentInDir(new Vector3(v.x, v.y, 0f));
        }
    }
}