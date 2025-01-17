

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

        public void Save() 
        {
            Debug.Log("Saved Asset");
            EditorUtility.SetDirty(this);
        }
    }

    [CustomEditor(typeof(SoundCollection))]
    public class SoundCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (SoundCollection)target;

                if(GUILayout.Button("Save", GUILayout.Height(40)))
                {
                    script.Save();
                }
            
        }
    }

}
