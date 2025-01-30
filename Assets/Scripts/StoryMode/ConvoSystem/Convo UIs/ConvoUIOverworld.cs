using System;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace StoryMode.ConvoSystem
{
    public class ConvoUIOverworld : ConvoUIBase
    {
        [SerializeField] private GameObject menuParent;
        [SerializeField] private Animator animator;
        
        [Header("Visuals")]
        [SerializeField] private Image[] portraits;
        [SerializeField] private TextMeshProUGUI[] names;
        [SerializeField] private Outline[] nameOutlines;
        [SerializeField] private Animator[] nameAnimators;
        [SerializeField] private List<Vector3> initialPositions = new();

        void Start()
        {
            menuParent.SetActive(false);

            foreach (Image i in portraits)
            {
                initialPositions.Add(i.transform.position);
            }
        }

        // TODO Actor portriats and name tags
        public override void StartConvo(Convo c)
        {
            menuParent.SetActive(true);
            base.StartConvo(c);
            SetVisuals();
            for (int i = 0; i < nameAnimators.Length; i++)
            {
                nameAnimators[i].Play(i == currentLine.activeActorIndex ? "Showing" : "Hiding");
            }
            animator.Play("Show");
        }

        public override void NextLine()
        {
            base.NextLine();
            if (currentLineIndex < currentConvo.lines.Length) SetVisuals();
        }

        private void SetVisuals()
        {
            for (int i = 0; i < currentLine.actors.Length; i++)
            {
                float o = i == currentLine.activeActorIndex ? 1f : 0.5f;
                Actor a = currentLine.actors[i];
                portraits[i].sprite = a.sprite;
                portraits[i].color = new Color(1f, 1f, 1f, o);
                names[i].text = a.actorName;
                // TODO animations
                nameAnimators[i].ResetTrigger(i != currentLine.activeActorIndex ? "Show" : "Hide");
                nameAnimators[i].SetTrigger(i == currentLine.activeActorIndex ? "Show" : "Hide");
                nameOutlines[i].effectColor = a.color;

                portraits[i].transform.position = initialPositions[i] + a.spriteOffset;
            }
        }

        public override void EndConvo()
        {
            base.EndConvo();
            animator.Play("Hide");
        }

        public void CloseAnimationComplete()
        {
            // ewwww
            GameObject.Find("Player").GetComponent<OverworldPlayer>().SetState(OverworldPlayer.PlayerState.Normal);
            menuParent.SetActive(false);
        }
    }
}