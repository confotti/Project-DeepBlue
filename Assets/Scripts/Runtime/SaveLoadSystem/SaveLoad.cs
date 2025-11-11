using SaveLoadSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SaveLoadSystem
{
    public static class SaveLoad
    {
        public static UnityAction OnSaveGame;
        public static UnityAction<SaveData> OnLoadGame;

        private const string SaveDirectory = "/SaveData/";
        private static string fileName = "SaveGame.sav";

        public static SaveData currentSavedata;

        public static bool Save()
        {
            if (currentSavedata == null) currentSavedata = new SaveData();
            OnSaveGame?.Invoke();

            string dir = Application.persistentDataPath + SaveDirectory;

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonUtility.ToJson(currentSavedata, true);
            File.WriteAllText(dir + fileName, json);

#if UNITY_EDITOR
            GUIUtility.systemCopyBuffer = dir + fileName;
#endif

            return true;

        }

        public static async Task<SaveData> Load()
        {
            string fullPath = Application.persistentDataPath + SaveDirectory + fileName;
            currentSavedata = new SaveData();

            if (File.Exists(fullPath))
            {
                //string json = File.ReadAllText(fullPath);
                currentSavedata = JsonUtility.FromJson<SaveData>(File.ReadAllText(fullPath));

                OnLoadGame?.Invoke(currentSavedata);
            }
            else
            {
                Debug.Log("Save file does not exist!");
            }

            return currentSavedata;
        }

        public static void DeleteSaveData()
        {
            string fullPath = Application.persistentDataPath + SaveDirectory + fileName;

            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}