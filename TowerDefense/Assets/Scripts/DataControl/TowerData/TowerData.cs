using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerData
{
    public abstract class TowerData : ScriptableObject
    {
        public bool IsUnitTower => isUnitTower;
        public bool IsMagicTower => isMagicTower;

        public TowerType TowerType => towerType;
        [SerializeField] private TowerType towerType;

        public ushort TowerBuildCost => towerBuildCost;
        public byte ExtraBuildCost => extraBuildCost;
        public ushort TowerUpgradeCost => towerUpgradeCost;
        public byte ExtraUpgradeCost => extraUpgradeCost;

        [SerializeField] private bool isUnitTower;
        [SerializeField] private bool isMagicTower;

        [SerializeField, Range(0, 1000)] private ushort towerBuildCost;
        [SerializeField, Range(0, 255)] private byte extraBuildCost;
        [SerializeField, Range(0, 1000)] private ushort towerUpgradeCost;
        [SerializeField, Range(0, 255)] private byte extraUpgradeCost;

        public virtual void InitState()
        {
        }
    }
}