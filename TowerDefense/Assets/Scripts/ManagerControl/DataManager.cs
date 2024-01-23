using System;
using System.IO;
using UnityEngine;
using Utilities;

namespace ManagerControl
{
    [Serializable] public class SurvivedWave
    {
        public byte[] survivedWave = { 0, 0, 0, 0 };
    }

    public abstract class DataManager
    {
        public static int xp { get; set; }

        public static SurvivedWave survivedWaves { get; private set; }

        private static string _path;
        private static byte _difficultyLevel;
        private static byte _lastSurvivedWave;

        private const string Filename = "/waves.txt";

        public static void Init()
        {
            _path = Application.persistentDataPath + Filename;
            _difficultyLevel = 0;

            survivedWaves = new SurvivedWave();
            for (var i = 0; i < survivedWaves.survivedWave.Length; i++)
            {
                survivedWaves.survivedWave[i] = 0;
            }

            LoadData();
        }

        public static void SetLevel(byte index)
        {
            _difficultyLevel = index;
            _lastSurvivedWave = survivedWaves.survivedWave[_difficultyLevel - 1];
        }

        private static void SaveData()
        {
            var data = JsonUtility.ToJson(survivedWaves);
            File.WriteAllText(_path, data);
        }

        public static void LoadData()
        {
            if (!File.Exists(_path))
            {
                SaveData();
            }

            var data = File.ReadAllText(_path);
            survivedWaves = JsonUtility.FromJson<SurvivedWave>(data);
        }

        public static void SaveLastSurvivedWave()
        {
            // if (IsGameOver) return;
            // IsGameOver = true;
            if (_lastSurvivedWave < survivedWaves.survivedWave[_difficultyLevel - 1])
            {
                SaveData();
            }

            var prevXp = PlayerPrefs.GetInt(StringManager.Xp);
            var earnedXp = survivedWaves.survivedWave[_difficultyLevel - 1] *
                (survivedWaves.survivedWave[_difficultyLevel - 1] + 1) * _difficultyLevel / 2;
            PlayerPrefs.SetInt(StringManager.Xp, prevXp + earnedXp);
            xp = PlayerPrefs.GetInt(StringManager.Xp);
        }

        public static void UpdateSurvivedWave(byte wave)
        {
            survivedWaves.survivedWave[_difficultyLevel - 1] = wave;
        }

        public static void WaveDataInit()
        {
            for (var i = 0; i < survivedWaves.survivedWave.Length; i++)
            {
                survivedWaves.survivedWave[i] = 0;
            }

            SaveData();
        }
    }
}