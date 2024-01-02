using CustomEnumControl;
using UnityEngine;
using Utilities;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Summoning Tower Data")]
    public class SummoningTowerData : AttackTowerData
    {
        public int UnitHealth { get; private set; }
        public float UnitReSpawnTime { get; private set; }

        [SerializeField] private int initUnitHealth;
        [SerializeField] private float initReSpawnTime;

        public override void InitState()
        {
            base.InitState();
            AttackRange = InitRange;
            UnitReSpawnTime = initReSpawnTime;
            var health = PlayerPrefs.GetInt(StringManager.UnitHealthDic[TowerType]);
            if (health <= 0)
            {
                PlayerPrefs.SetInt(StringManager.UnitHealthDic[TowerType], initUnitHealth);
                UnitHealth = initUnitHealth;
            }
            else
            {
                UnitHealth = (ushort)health;
            }
        }

        public override void UpgradeData(TowerType type)
        {
            base.UpgradeData(type);
            var unitHealth = PlayerPrefs.GetInt(StringManager.UnitHealthDic[type]);
            UnitHealth += (ushort)(unitHealth + 50);
            PlayerPrefs.SetInt(StringManager.UnitHealthDic[type], UnitHealth);
        }
    }
}