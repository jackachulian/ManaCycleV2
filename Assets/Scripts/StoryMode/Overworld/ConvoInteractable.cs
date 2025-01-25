using StoryMode.ConvoSystem;
using UnityEngine;

namespace StoryMode.Overworld
{
    public class ConvoInteractable : OverworldInteractable
    {
        [SerializeField] private ConvoUI convoUI;
        [SerializeField] private Convo convo;

        public override void OnInteract(OverworldPlayerInteracter interacter)
        {
           convoUI.StartConvo(convo);
        }
    }
}