using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace StoryMode.Overworld
{
    public class InteractionManager : MonoBehaviour
    {
        private List<OverworldInteractable> interactablesInRange = new();
        public delegate void InteractableChangeCallback(OverworldInteractable? interactable);
        public event InteractableChangeCallback? InteractableChangeNotifier;

        void OnCollisionEnter(Collision collision)
        {
            OverworldInteractable oi = collision.transform.GetComponent<OverworldInteractable>();

            if (oi)
            {
                interactablesInRange.Add(oi);
            }

            InteractableChangeNotifier.Invoke(oi);
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

