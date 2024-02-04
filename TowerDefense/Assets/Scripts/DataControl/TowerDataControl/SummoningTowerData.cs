using BackendControl;
using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Summoning Tower Data")]
    public class SummoningTowerData : AttackTowerData
    {
        public int curUnitHealth { get; private set; }
        [field: SerializeField] public float initReSpawnTime { get; private set; }

        [SerializeField] private int initUnitHealth;

        public override void InitState()
        {
            base.InitState();
            curUnitHealth = BackendGameData.userData.towerLevelTable[towerType.ToString()] * 50 + initUnitHealth;
        }

        public override void UpgradeData(int towerLv)
        {
            base.UpgradeData(towerLv);
            curUnitHealth = BackendGameData.userData.towerLevelTable[towerType.ToString()] * 50 + initUnitHealth;
        }
    }
}