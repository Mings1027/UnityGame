using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl.TowerDataControl
{
    public abstract class TowerData : ScriptableObject
    {
        public bool IsUnitTower => isUnitTower;
        public bool IsMagicTower => isMagicTower;

        public TowerType TowerType => towerType;
        [SerializeField] private TowerType towerType;

        public ushort TowerBuildGold => towerBuildGold;
        public byte ExtraBuildGold => extraBuildGold;
        public ushort TowerUpgradeGold => towerUpgradeGold;
        public byte ExtraUpgradeGold => extraUpgradeGold;

        [SerializeField] private bool isUnitTower;
        [SerializeField] private bool isMagicTower;

        [FormerlySerializedAs("towerBuildCost")] [SerializeField, Range(0, 1000)] private ushort towerBuildGold;
        [FormerlySerializedAs("extraBuildCost")] [SerializeField, Range(0, 255)] private byte extraBuildGold;
        [FormerlySerializedAs("towerUpgradeCost")] [SerializeField, Range(0, 1000)] private ushort towerUpgradeGold;
        [FormerlySerializedAs("extraUpgradeCost")] [SerializeField, Range(0, 255)] private byte extraUpgradeGold;

        public virtual void InitState()
        {
        }
    }
}