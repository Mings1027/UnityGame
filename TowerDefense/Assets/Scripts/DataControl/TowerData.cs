using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    public abstract class TowerData : ScriptableObject
    {
        public bool IsUnitTower => isUnitTower;
        public bool IsMagicTower => isMagicTower;

        public MeshFilter[] TowerMeshes => towerMeshes;

        public ushort BaseDamage { get; private set; }
        public byte AttackRange { get; private set; }
        public ushort AttackRpm { get; private set; }

        public GameObject Tower => tower;
        public TowerType TowerType => towerType;
        public ushort TowerBuildCost => towerBuildCost;
        public ushort TowerUpgradeCost => towerUpgradeCost;

        [SerializeField] private bool isUnitTower;
        [SerializeField] private bool isMagicTower;

        [SerializeField] private ushort initDamage;
        [SerializeField] private byte initRange;
        [SerializeField] private ushort initRpm;
        [SerializeField] private MeshFilter[] towerMeshes;

        [SerializeField] private GameObject tower;
        [SerializeField] private TowerType towerType;
        [SerializeField, Range(0, 1000)] private ushort towerBuildCost;
        [SerializeField, Range(0, 1000)] private ushort towerUpgradeCost;

        public virtual void InitState()
        {
            BaseDamage = initDamage;
            AttackRange = initRange;
            AttackRpm = initRpm;
        }

        public virtual void UpgradeData()
        {
            BaseDamage += 5;
            AttackRange += 1;
        }
    }
}