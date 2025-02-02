using UnityEngine;

namespace StoryMode.Overworld
{
    public class LevelInteractable : OverworldInteractable
    {
        [SerializeField] private Level _level;
        public Level level => _level;

        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
            Debug.Log("Playing level (" + _level + ")");

            if (OverworldPlayer.Instance) OverworldPlayer.Instance.playerInput.enabled = false;

            _level.StartLevelBattle();
        }

        public override void OnInteractionRangeEntered()
        {
            
        }

        public override void OnInteractionRangeExited()
        {
            
        }
    }
}