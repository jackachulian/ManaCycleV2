using UnityEngine;

namespace StoryMode.Overworld
{
    public abstract class OverworldInteractable : MonoBehaviour
    {
        public string interactableName {get; private set;}
        public abstract void OnInteract();
    }
}