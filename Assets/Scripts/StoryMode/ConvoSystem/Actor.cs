using UnityEngine;

namespace StoryMode.ConvoSystem
{
    [CreateAssetMenu(fileName = "Actor", menuName = "ManaCycle/Convo Actor")]
    public class Actor : ScriptableObject
    {
        public string actorName;
        public Sprite sprite;
        public Vector3 spriteOffset;
        public Color color;
        // TODO sfx

    }
}