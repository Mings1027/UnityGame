using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    public abstract class TowerData : ScriptableObject
    {
        public bool IsUnitTower => isUnitTower;
        public bool IsMagicTower => isMagicTower;

        public MeshFilter[] TowerMeshes => towerMeshes;

        public int BaseDamage { get; private set; }
        public byte AttackRange { get; private set; }
        public ushort AttackRpm { get; private set; }

        public GameObject Tower => tower;
        public TowerType TowerType => towerType;
        public int TowerBuildCost => towerBuildCost;
        public int TowerUpgradeCost => towerUpgradeCost;

        [SerializeField] private bool isUnitTower;
        [SerializeField] private bool isMagicTower;

        [SerializeField] private int initDamage;
        [SerializeField] private byte initRange;
        [SerializeField] private ushort initRpm;
        [SerializeField] private MeshFilter[] towerMeshes;

        [SerializeField] private GameObject tower;
        [SerializeField] private TowerType towerType;
        [SerializeField, Range(0, 200)] private int towerBuildCost;
        [SerializeField, Range(0, 200)] private int towerUpgradeCost;

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