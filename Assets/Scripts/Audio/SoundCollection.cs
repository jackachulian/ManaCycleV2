

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Audio
{
    [Serializable] public class SoundCollectionDictionary : SerializableDictionary<string, AudioClip> {}

    [CreateAssetMenu(fileName = "Sound Collection", menuName = "ManaCycle/SoundCollection", order = 1)]
    public class SoundCollection : ScriptableObject
    {
        public SoundCollectionDictionary sounds;

        public void Save() 
        {
            #if UNITY_EDITOR
            Debug.Log("Saved Asset");
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    #if UNITY_EDITOR
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
    #endif
}
