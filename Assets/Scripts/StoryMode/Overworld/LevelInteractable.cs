using UnityEngine;

namespace StoryMode.Overworld
{
    public class LevelInteractable : OverworldInteractable
    {
        [SerializeField] private Level level;

        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
            Debug.Log("Playing level (" + level.id + ")");

            if (OverworldPlayer.Instance) OverworldPlayer.Instance.playerInput.enabled = false;

            level.StartLevelBattle();
        }
    }
}