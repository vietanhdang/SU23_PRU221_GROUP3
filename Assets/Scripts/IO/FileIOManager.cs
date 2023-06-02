using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.IO
{
    public class FileIOManager : MonoBehaviour
    {
        private string filePath; // The path to the file to write to.
        public bool isReady = false;

        public GameData gameData;
        public void Start()
        {
            InitializeFilePath();
        }
        public void InitializeFilePath()
        {
            filePath = Application.persistentDataPath + "/data.json";
        }
        public GameData LoadGameData()
        {
            filePath = Application.persistentDataPath + "/data.json";
            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath);
                GameData gameData = JsonConvert.DeserializeObject<GameData>(data);
                return gameData;
            }
            else
            {
                Debug.LogError("File doesn't exist.");
                return null;
            }
        }

        public void SaveGame(GameData gameData)
        {
            string json = JsonConvert.SerializeObject(gameData);
            File.WriteAllText(filePath, json);
        }

        public void DeleteFile()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
