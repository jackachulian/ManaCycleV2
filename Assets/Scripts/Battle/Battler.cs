using System;
using UnityEngine;

namespace Battle {
    [CreateAssetMenu(fileName = "Battler", menuName = "ManaCycle/Battler")]
    public class Battler : ScriptableObject {
        [SerializeField] private string _battlerId;
        public string battlerId => _battlerId;

        [SerializeField] public string displayName;

        [SerializeField] public Sprite sprite;

        /// <summary>Offset of the portrait in the battle view</summary>
        [SerializeField] public Vector2 portraitOffset;

        // used for the attack popup gradients
        [SerializeField] public Material gradientMat;
        // colors for various ui designs
        [SerializeField] public Color mainColor = Color.white;
        [SerializeField] public Color altColor = Color.black;
        // crossover logo for char select
        [SerializeField] public Sprite gameLogo;
    }
}