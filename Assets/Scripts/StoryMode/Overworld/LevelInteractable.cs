using UnityEngine;

namespace StoryMode.Overworld
{
    public class LevelInteractable : OverworldInteractable
    {
        void Awake()
        {
            
        }
        
        public override void OnInteract()
        {
            Debug.Log("Implement This! (" + interactableName + ")");
        }
    }
}