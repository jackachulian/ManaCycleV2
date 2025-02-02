using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SaveData {
    public class SaveDataManager {
        // if true, store save data in serialized binary format. if false, save in json format
        public static readonly bool useBinaryMode = true;

        /// <summary>
        /// Current save data loaded from file and stored in memory, referenced statically here
        /// </summary>
        public static SaveData saveData {get; private set;}

        public static void SaveToFile(string path = "save.data") {
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

        public static void LoadFromFile(string path = "save.data") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);

            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    saveData = (SaveData)formatter.Deserialize(stream);
                }
            } else {
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    saveData = JsonUtility.FromJson<SaveData>(json);
                }
                else
                {
                    Debug.LogError("File not found!");
                    return;
                }
            }

            Debug.Log($"Read save data from: {fullPath}");
        }
    }
}