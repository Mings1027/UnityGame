using BackendControl;
using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    public abstract class TowerData : ScriptableObject
    {
        [field: SerializeField] public TowerType towerType { get; private set; }
        [field: SerializeField] public bool isUnitTower { get; private set; }
        public byte curRange { get; private set; }

        [field: SerializeField, Range(0, 1000)]
        public ushort towerBuildCost { get; private set; }

        [field: SerializeField, Range(0, 255)] public byte extraBuildCost { get; private set; }

        [field: SerializeField, Range(0, 1000)]
        public ushort towerUpgradeCost { get; private set; }

        [field: SerializeField, Range(0, 255)] public byte extraUpgradeCost { get; private set; }
        [SerializeField] private byte initRange;

        public virtual void InitState()
        {
            var towerLevel = BackendGameData.userData.towerLevelTable[towerType.ToString()];
            curRange = (byte)(towerLevel + initRange);
        }

        public virtual void UpgradeData(int towerLv)
        {
            curRange = (byte)(towerLv + initRange);
        }
    }
}