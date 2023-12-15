using System;
using System.IO;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    [Serializable]
    public class SurvivedWave
    {
        public byte[] survivedWave = { 0, 0, 0, 0 };
    }

    public abstract class DataManager
    {
        public static int Xp { get; set; }
        public static SurvivedWave SurvivedWaves { get; private set; }
        private static bool IsGameOver { get; set; }
        private static string _path;
        private static byte _difficultyLevel;
        private static byte _lastSurvivedWave;

        private const string Filename = "/waves.txt";

        public static void Init()
        {
            IsGameOver = false;
            _path = Application.persistentDataPath + Filename;
            _difficultyLevel = 0;

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
            if (IsGameOver) return;
            IsGameOver = true;
            if (_lastSurvivedWave < SurvivedWaves.survivedWave[_difficultyLevel - 1])
            {
                SaveData();
            }

            var prevXp = PlayerPrefs.GetInt(StringManager.Xp);
            var xp = SurvivedWaves.survivedWave[_difficultyLevel - 1] *
                (SurvivedWaves.survivedWave[_difficultyLevel - 1] + 1) * _difficultyLevel / 2;
            PlayerPrefs.SetInt(StringManager.Xp, prevXp + xp);
            Xp = PlayerPrefs.GetInt(StringManager.Xp);
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