using UnityEngine;

namespace StoryMode.Overworld
{
    public class LevelInteractable : OverworldInteractable
    {
        [SerializeField] private Level _level;
        public Level level => _level;

        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
            

            if (level) {
                Debug.Log("Playing level (" + level + ")");
                LevelDetails.Instance.OpenDetailsWindow(level);
            } else {
                Debug.Log("Interacted with levelinteractable with no level defined");
            }

            // _level.StartLevelBattle();
        }

        public override void OnInteractionRangeEntered()
        {
            
        }

        public override void OnInteractionRangeExited()
        {
            
        }
    }
}