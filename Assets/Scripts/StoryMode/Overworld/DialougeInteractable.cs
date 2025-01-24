using UnityEngine;

namespace StoryMode.Overworld
{
    public class DialougeInteractable : OverworldInteractable
    {
        // TODO Implement dialouge system
        public string temp;

        public override void OnInteract()
        {
            Debug.Log(temp);
        }
    }
}