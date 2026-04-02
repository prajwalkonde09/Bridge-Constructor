using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Eduzo.Games.BridgeConstruction.Data
{
    [System.Serializable]
    public class BridgeConstructionDatabase
    {
        public List<BridgeConstructionQuestion> Questions = new List<BridgeConstructionQuestion>();
    }

    public static class BridgeConstructionDatabaseManager
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "bridge_questions.json");

        public static BridgeConstructionDatabase Database = new BridgeConstructionDatabase();

        public static void Save()
        {
            string json = JsonUtility.ToJson(Database, true);
            File.WriteAllText(FilePath, json);
        }

        public static void Load()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                Database = JsonUtility.FromJson<BridgeConstructionDatabase>(json);
            }
            else
            {
                Database = new BridgeConstructionDatabase();
            }
        }

        public static void Clear()
        {
            Database.Questions.Clear();
            Save();
        }
    }
}