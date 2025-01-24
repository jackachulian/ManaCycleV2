using UnityEngine;
using System.Collections.Generic;

namespace StoryMode.Overworld
{
    public class InteractionManager : MonoBehaviour
    {
        private List<OverworldInteractable> interactablesInRange = new();
        public delegate void InteractableChangeCallback(OverworldInteractable? interactable);
        public event InteractableChangeCallback interactableChangeNotifier;

        void OnCollisionEnter(Collision collision)
        {
            OverworldInteractable oi = collision.transform.GetComponent<OverworldInteractable>();

            if (oi)
            {
                interactablesInRange.Add(oi);
            }

            interactableChangeNotifier.Invoke(oi);
        }

        void OnCollisionExit(Collision collision)
        {
            OverworldInteractable oi = collision.transform.GetComponent<OverworldInteractable>();

            if (oi)
            {
                interactablesInRange.Remove(oi);
            }

            interactableChangeNotifier.Invoke(
                interactablesInRange.Count > 0 ? interactablesInRange[-1] : null
            );
        }
    }
}

