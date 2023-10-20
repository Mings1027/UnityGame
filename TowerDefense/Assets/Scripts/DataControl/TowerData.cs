using System;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    public abstract class TowerData : ScriptableObject
    {
        public bool IsMagicTower => isMagicTower;
        public TowerInfoData[] TowerLevels => towerLevels;
        public GameObject Tower => tower;
        public PoolObjectKey PoolObjectKey => poolObjectKey;
        public TowerType TowerType => towerType;
        public int TowerBuildCost => towerBuildCost;
        public int TowerUpgradeCost => towerUpgradeCost;

        [SerializeField] private bool isMagicTower;
        [SerializeField] private TowerInfoData[] towerLevels;
        [SerializeField] private GameObject tower;
        [SerializeField] private PoolObjectKey poolObjectKey;
        [SerializeField] private TowerType towerType;
        [SerializeField, Range(0, 200)] private int towerBuildCost;
        [SerializeField, Range(0, 200)] private int towerUpgradeCost;

        [Serializable]
        public class TowerInfoData
        {
            public MeshFilter towerMesh;
            public int damage;
            public byte attackRange;
            public float attackDelay;
        }
    }
}