using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class UnitTowerData : TowerData
    {
        public int UnitHealth { get; private set; }
        public float UnitReSpawnTime { get; private set; }

        [SerializeField] private int initUnitHealth;
        [SerializeField] private float initReSpawnTime;

        public override void InitState()
        {
            base.InitState();
            UnitReSpawnTime = initReSpawnTime;
            UnitHealth = initUnitHealth;
        }

        public override void UpgradeData()
        {
            base.UpgradeData();
            UnitHealth += 50;
        }
    }
    
}