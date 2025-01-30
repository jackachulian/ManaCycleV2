using System;
using UnityEngine;

namespace StoryMode.ConvoSystem
{
    public class ConvoManager : MonoBehaviour
    {
        [NonSerialized] public static ConvoManager Instance = null;
        void Awake() {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static ConvoUIBase currentConvoUI;
    }
}