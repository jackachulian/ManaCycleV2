using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SaveDataSystem {
    public class SaveDataManager : MonoBehaviour { 
        public static SaveDataManager Instance {get; private set;}

        // if true, store save data in serialized binary format. if false, save in json format
        public static readonly bool useBinaryMode = true;

        /// <summary>
        /// Current save data loaded from file and stored in memory, referenced statically here
        /// </summary>
        public SaveData saveData {get; private set;}

        void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadFromFile();
        }

        void OnApplicationQuit() {
            SaveToFile();
        }

        public void SaveToFile(string path = "save.data") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);
            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    formatter.Serialize(stream, saveData);
                }
            } else {
                string json = JsonUtility.ToJson(saveData, true); // Convert to JSON string
                File.WriteAllText(fullPath, json);
            }

            Debug.Log("Wrote save data to "+fullPath);
        }

        public void LoadFromFile(string path = "save.data") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);

            if (File.Exists(fullPath))
            {
                if (useBinaryMode) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        saveData = (SaveData)formatter.Deserialize(stream);
                    }
                } else {
                    string json = File.ReadAllText(fullPath);
                    saveData = JsonUtility.FromJson<SaveData>(json);
                }

                Debug.Log($"Read save data from: {fullPath}");
            } else {
                Debug.LogWarning("Save file not found, creating new save data obj");
                saveData = new SaveData();
                return;
            }
        }
    }
}