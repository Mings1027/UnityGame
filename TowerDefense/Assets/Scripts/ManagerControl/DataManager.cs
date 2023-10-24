using System.Collections.Generic;
using System.IO;
using CustomEnumControl;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class DataManager : MonoBehaviour
    {
        private static string _path;
        public static Dictionary<TowerType, int> damageDic { get; private set; }

        private void Awake()
        {
            _path = Application.persistentDataPath + "/Damage";

            if (Directory.Exists(_path))
            {
                LoadDamageData();
            }
            else
            {
                Directory.CreateDirectory(_path);
                damageDic = new Dictionary<TowerType, int>
                {
                    { TowerType.Assassin, 0 }, { TowerType.Ballista, 0 },
                    { TowerType.Canon, 0 }, { TowerType.Defender, 0 },
                    { TowerType.Wizard, 0 },
                };
                var jsonData = DictionaryJsonUtility.ToJson(damageDic, true);
                File.WriteAllText(_path + "/damage.txt", jsonData);
            }
        }

        public static void SumDamage(TowerType towerTypeEnum, int damage)
        {
            damageDic[towerTypeEnum] += damage;
        }

        public static void InitDamage()
        {
            foreach (var damage in damageDic.Keys)
            {
                damageDic[damage] = 0;
            }
        }

        public static void SaveDamageData()
        {
            var data = DictionaryJsonUtility.ToJson(damageDic);
            File.WriteAllText(_path + "/damage.txt", data);
        }

        public static void LoadDamageData()
        {
            var data = File.ReadAllText(_path + "/damage.txt");
            damageDic = DictionaryJsonUtility.FromJson<TowerType, int>(data);
        }
    }
}