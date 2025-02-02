using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StoryMode.Overworld
{
    public class InteractionManager : MonoBehaviour
    {
        public List<OverworldInteractable> interactablesInRange {get; private set;} = new();
        public delegate void InteractableChangeCallback(OverworldInteractable? interactable);
        public event InteractableChangeCallback? InteractableChangeNotifier;

        public OverworldInteractable nearest {get; private set;}

        /// <summary>
        /// Called when the nearest interactable to the player changes.
        /// Can be null, in which case there is no interactable object in range to the player.
        /// </summary>
        public event Action<OverworldInteractable> onNearestChanged;

        void OnEnable() {

        }

        void OnDisable() {
            if (nearest) nearest.OnInteractionRangeExited();
            nearest = null;
            onNearestChanged.Invoke(nearest);
        }

        void Update() {
            // Constantly keep track of the nearest interactable
            var previousNearest = nearest;

            nearest = null;
            float closestSqrDistance = float.MaxValue;

            for (int i = 0; i < interactablesInRange.Count; i++) {
                var interactable = interactablesInRange[i];
                float sqrDistance = (interactable.transform.position - transform.position).magnitude;

                if (sqrDistance < closestSqrDistance) {
                    nearest = interactable;
                    closestSqrDistance = sqrDistance;
                }
            }

            if (nearest != previousNearest) {
                if (previousNearest) previousNearest.OnInteractionRangeExited();
                if (nearest) nearest.OnInteractionRangeEntered();
                onNearestChanged.Invoke(nearest);
            }
        }


        void OnCollisionEnter(Collision collision)
        {
            OverworldInteractable oi = collision.transform.GetComponent<OverworldInteractable>();

            if (oi)
            {
                interactablesInRange.Add(oi);
            }

            InteractableChangeNotifier?.Invoke(oi);
        }

        void OnCollisionExit(Collision collision)
        {
            OverworldInteractable oi = collision.transform.GetComponent<OverworldInteractable>();

            if (oi)
            {
                interactablesInRange.Remove(oi);
            }

            InteractableChangeNotifier?.Invoke(
                interactablesInRange.Count > 0 ? interactablesInRange.Last() : null
            );
        }
    }
}

