using System;
using StoryMode.Overworld;
using UnityEngine;

namespace StoryMode.ConvoSystem
{
    public class OverworldConvoUI : ConvoUI
    {
        // TODO Actor portriats and name tags
        public override void StartConvo(Convo c)
        {
            Debug.Log("Staring Overworld Convo!");
            base.StartConvo(c);
        }

        public override void EndConvo()
        {
            // ewwww
            GameObject.Find("Player").GetComponent<OverworldPlayer>().SetState(OverworldPlayer.PlayerState.Normal);
            base.EndConvo();
        }
    }
}