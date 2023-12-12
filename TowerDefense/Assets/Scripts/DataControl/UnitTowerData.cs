using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Battle Tower Data/Unit Tower Data")]
    public class UnitTowerData : BattleTowerData
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
            UnitHealth = initUnitHealth;
        }

        public override void UpgradeData(TowerType type)
        {
            base.UpgradeData(type);
            UnitHealth += 50;
        }
    }
}