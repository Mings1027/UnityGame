using System;
using System.IO;
using UnityEngine;

namespace ManagerControl
{
    [Serializable]
    public class SurvivedWave
    {
        public byte[] survivedWave = { 0, 0, 0, 0 };
    }

    public static class DataManager
    {
        public static SurvivedWave SurvivedWaves { get; private set; }

        private static string _path;
        private static byte _difficultyLevel;
        private static byte _lastSurvivedWave;

        private const string Filename = "/waves.txt";

        public static void Init()
        {
            _path = Application.persistentDataPath + Filename;

            SurvivedWaves = new SurvivedWave();
            LoadData();
        }

        public static void SetLevel(byte index)
        {
            _difficultyLevel = index;
            _lastSurvivedWave = SurvivedWaves.survivedWave[_difficultyLevel - 1];
        }

        private static void SaveData()
        {
            var data = JsonUtility.ToJson(SurvivedWaves);
            File.WriteAllText(_path, data);
        }

        public static void LoadData()
        {
            if (!File.Exists(_path))
            {
                SaveData();
            }

            var data = File.ReadAllText(_path);
            SurvivedWaves = JsonUtility.FromJson<SurvivedWave>(data);
        }

        public static void SaveLastSurvivedWave()
        {
            if (_lastSurvivedWave < SurvivedWaves.survivedWave[_difficultyLevel - 1])
                SaveData();
        }

        public static void UpdateSurvivedWave(byte wave)
        {
            SurvivedWaves.survivedWave[_difficultyLevel - 1] = wave;
        }

        public static void WaveDataInit()
        {
            for (int i = 0; i < SurvivedWaves.survivedWave.Length; i++)
            {
                SurvivedWaves.survivedWave[i] = 0;
            }

            SaveData();
        }
    }
}