using System;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace StoryMode.ConvoSystem
{
    public class OverworldConvoUI : ConvoUI
    {
        [SerializeField] private GameObject menuParent;
        
        [Header("Visuals")]
        [SerializeField] private Image[] portraits;
        [SerializeField] private TextMeshProUGUI[] names;
        [SerializeField] private Outline[] nameOutlines;
        [SerializeField] private List<Vector3> initialPositions = new();

        void Start()
        {
            menuParent.SetActive(false);

            foreach (Image i in portraits)
            {
                initialPositions.Add(i.transform.position);
            }

            SetVisuals();
        }

        // TODO Actor portriats and name tags
        public override void StartConvo(Convo c)
        {
            menuParent.SetActive(true);
            base.StartConvo(c);

            SetVisuals();
        }

        public override void NextLine()
        {
            base.NextLine();
            SetVisuals();
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
                nameOutlines[i].gameObject.SetActive(i == currentLine.activeActorIndex);
                nameOutlines[i].effectColor = a.color;

                portraits[i].transform.position = initialPositions[i] + a.spriteOffset;
            }
        }

        public override void EndConvo()
        {
            // ewwww
            GameObject.Find("Player").GetComponent<OverworldPlayer>().SetState(OverworldPlayer.PlayerState.Normal);
            base.EndConvo();
            menuParent.SetActive(false);

            SetVisuals();
        }

        public void CloseAnimationComplete()
        {
            menuParent.SetActive(false);
        }
    }
}