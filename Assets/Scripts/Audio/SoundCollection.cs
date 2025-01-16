

using UnityEngine;
using System;

namespace Audio
{
    [Serializable] public class SoundCollectionDictionary : SerializableDictionary<string, AudioClip> {}

    [CreateAssetMenu(fileName = "Sound Collection", menuName = "ManaCycle/SoundCollection", order = 1)]
    public class SoundCollection : ScriptableObject
    {
        public SoundCollectionDictionary sounds;
    }

}
