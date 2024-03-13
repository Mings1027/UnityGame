using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    public abstract class TowerData : ScriptableObject
    {
        [field: SerializeField] public TowerType towerType { get; private set; }
        [field: SerializeField] public bool isUnitTower { get; private set; }

        [field: SerializeField, Range(0, 1000)]
        public ushort towerBuildCost { get; private set; }

        [field: SerializeField, Range(0, 255)] public byte extraBuildCost { get; private set; }

        [field: SerializeField, Range(0, 1000)]
        public ushort towerUpgradeCost { get; private set; }

        [field: SerializeField, Range(0, 255)] public byte extraUpgradeCost { get; private set; }

        public virtual void InitState()
        {
        }
    }
}