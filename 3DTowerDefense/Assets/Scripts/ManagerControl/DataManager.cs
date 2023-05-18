using System.IO;
using UnityEngine;

namespace ManagerControl
{
    public static class DataManager
    {
        public static void SaveDataToJson<T>(string fileName)
        {
            var jsonData = JsonUtility.ToJson(typeof(T), true);
            var path = Path.Combine(Application.dataPath, fileName);
            File.WriteAllText(path, jsonData);
        }

        public static T LoadDataFromJson<T>(string fileName)
        {
            var path = Path.Combine(Application.dataPath, fileName);
            var jsonData = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(jsonData);
        }
    }
}