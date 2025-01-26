using StoryMode.ConvoSystem;
using UnityEngine;

namespace StoryMode.Overworld
{
    public class ConvoInteractable : OverworldInteractable
    {
        [SerializeField] private ConvoUIBase convoUI;
        [SerializeField] private Convo convo;

        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
            // don't start new convo if already in one
            if (ConvoManager.currentConvoUI != null) return;
            convoUI.StartConvo(convo);
            interacter.GetComponent<OverworldPlayer>()?.SetState(OverworldPlayer.PlayerState.Convo);
        }
    }
}