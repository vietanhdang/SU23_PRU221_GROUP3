using Assets.Scripts.CustomException;
using Newtonsoft.Json;
using System;
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

        public GameData LoadDefaultGameData()
        {
            try
            {
                string data = File.ReadAllText("Assets/Scripts/IO/defaultData.json");
                data = null;
                if (data == null)
                {
                    throw new ExceptionHandling("File doesn't exist.", "", DateTime.Now, "43");
                }
                GameData gameData = JsonConvert.DeserializeObject<GameData>(data);
                return gameData;
            }
            catch (ExceptionHandling ex)
            {
                ex.Handle();
                return null;
            }catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
            
        }

        public void SaveGame(GameData gameData)
        {
            filePath = Application.persistentDataPath + "/data.json";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            string data = JsonConvert.SerializeObject(gameData);
            File.WriteAllText(filePath, data);
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
