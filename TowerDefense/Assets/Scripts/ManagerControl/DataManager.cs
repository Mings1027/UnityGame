using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameControl;
using TMPro;
using UnityEngine;

namespace ManagerControl
{
    public class DataManager : MonoBehaviour
    {
        private static string _path;
        private static Dictionary<string, int> _damageDic;

        private void Awake()
        {
            _path = Application.persistentDataPath + "/damage";

            if (File.Exists(_path))
            {
                LoadDamageData();
            }
            else
            {
                _damageDic = new Dictionary<string, int>
                {
                    { "Ballista", 0 },
                    { "Assassin", 0 },
                    { "Canon", 0 },
                    { "Defender", 0 },
                    { "Mage", 0 }
                };
            }
        }

        public static void SumDamage(string towerType, int damage)
        {
            _damageDic[towerType] += damage;
        }

        public static void SaveDamageData()
        {
            var data = DictionaryJsonUtility.ToJson(_damageDic);
            File.WriteAllText(_path, data);
            TowerManager.Instance.SetDamageText(_damageDic);
        }

        public static void LoadDamageData()
        {
            var data = File.ReadAllText(_path);
            _damageDic = DictionaryJsonUtility.FromJson<string, int>(data);
        }
    }
}