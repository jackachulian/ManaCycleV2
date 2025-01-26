using System;
using StoryMode.Overworld;
using UnityEngine;

namespace StoryMode.ConvoSystem
{
    public class OverworldConvoUI : ConvoUI
    {
        [SerializeField] private GameObject menuParent;

        void Start()
        {
            menuParent.SetActive(false);
        }

        // TODO Actor portriats and name tags
        public override void StartConvo(Convo c)
        {
            menuParent.SetActive(true);
            base.StartConvo(c);
        }

        public override void EndConvo()
        {
            // ewwww
            GameObject.Find("Player").GetComponent<OverworldPlayer>().SetState(OverworldPlayer.PlayerState.Normal);
            base.EndConvo();
            menuParent.SetActive(false);
        }

        public void CloseAnimationComplete()
        {
            menuParent.SetActive(false);
        }
    }
}