

using UnityEngine;
using System;
using UnityEditor;

namespace Audio
{
    [Serializable] public class SoundCollectionDictionary : SerializableDictionary<string, AudioClip> {}

    [CreateAssetMenu(fileName = "Sound Collection", menuName = "ManaCycle/SoundCollection", order = 1)]
    public class SoundCollection : ScriptableObject
    {
        public SoundCollectionDictionary sounds;
        public SoundCollectionDictionary Sounds
        {
            get => sounds;
            set
            {
                Debug.Log(EditorUtility.IsDirty(this));
                Undo.RecordObject(this, "Change");
            
                sounds = value;
                // Undo.FlushUndoRecordObjects();
                Debug.Log(EditorUtility.IsDirty(this));
            }
        }
    }

}
