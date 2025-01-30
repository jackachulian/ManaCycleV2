using UnityEngine;

namespace StoryMode.Overworld
{
    public class LevelInteractable : OverworldInteractable
    {        
        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
            Debug.Log("Implement This! (" + interactableName + ")");
        }
    }
}