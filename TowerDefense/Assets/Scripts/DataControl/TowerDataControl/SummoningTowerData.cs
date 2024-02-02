using BackendControl;
using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Summoning Tower Data")]
    public class SummoningTowerData : AttackTowerData
    {
        public int curUnitHealth { get; private set; }
        [field: SerializeField] public int initUnitHealth { get; private set; }
        [field: SerializeField] public float initReSpawnTime { get; private set; }

        public override void InitState()
        {
            base.InitState();
            curUnitHealth = (BackendGameData.userData.towerLevelTable[towerType.ToString()] + 1) * initUnitHealth;
        }

        public override void UpgradeData(int towerLv)
        {
            base.UpgradeData(towerLv);
            curUnitHealth = (BackendGameData.userData.towerLevelTable[towerType.ToString()] + 1) * initUnitHealth;
        }
    }
}