using System;
using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public abstract class TowerData : ScriptableObject
    {
        public TowerInfoData[] TowerLevels => towerLevels;
        public GameObject Tower => tower;
        public PoolObjectKey PoolObjectKey => poolObjectKey;
        public TowerType TowerType => towerType;
        public ushort TowerBuildGold => towerBuildGold;
        public ushort TowerUpgradeGold => towerUpgradeGold;

        [SerializeField] private TowerInfoData[] towerLevels;
        [SerializeField] private GameObject tower;
        [SerializeField] private PoolObjectKey poolObjectKey;
        [SerializeField] private TowerType towerType;
        [SerializeField] private ushort towerBuildGold;
        [SerializeField] private ushort towerUpgradeGold;

        [Serializable]
        public class TowerInfoData
        {
            public MeshFilter towerMesh;
            public ushort damage;
            public byte attackRange;
            public float attackDelay;
        }
    }
}