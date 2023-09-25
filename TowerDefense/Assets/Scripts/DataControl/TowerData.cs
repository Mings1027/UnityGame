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
        public TowerInfoData[] towerLevels;
        public GameObject tower;
        public PoolObjectKey poolObjectKey;
        public TowerType towerType;
        public int towerBuildGold;
        public int towerUpgradeGold;

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