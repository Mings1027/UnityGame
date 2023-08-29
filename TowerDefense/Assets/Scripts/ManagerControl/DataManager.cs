using System;
using System.Collections.Generic;
using System.IO;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class DataManager : Singleton<DataManager>
    {
        private string _path;

        private Dictionary<string, int> _damageDic;

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


        public void SumDamage(string towerType, int damage)
        {
            _damageDic[towerType] += damage;
        }

        public void SaveDamageData()
        {
            var data = DictionaryJsonUtility.ToJson(_damageDic);
            File.WriteAllText(_path, data);
        }

        public void LoadDamageData()
        {
            var data = File.ReadAllText(_path);
            _damageDic = DictionaryJsonUtility.FromJson<string, int>(data);
        }
    }
}