using System;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    [CreateAssetMenu]
    public class TowerData : ScriptableObject
    {
        public TowerInfoData[] TowerLevels => towerLevels;
        public GameObject Tower => tower;
        public PoolObjectKey PoolObjectKey => poolObjectKey;
        public TowerType TowerType => towerType;
        public int TowerBuildGold => towerBuildGold;
        public int TowerUpgradeGold => towerUpgradeGold;

        [SerializeField] private TowerInfoData[] towerLevels;
        [SerializeField] private GameObject tower;
        [SerializeField] private PoolObjectKey poolObjectKey;
        [SerializeField] private TowerType towerType;
        [SerializeField] private int towerBuildGold;
        [SerializeField] private int towerUpgradeGold;

        [Serializable]
        public class TowerInfoData
        {
            public MeshFilter towerMesh;
            public int damage;
            public int attackRange;
            public float attackDelay;
        }
    }
}