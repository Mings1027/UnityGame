using System;
using ManagerControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class TowerData : ScriptableObject
    {
        public GameObject tower;
        public TowerInfoData[] towerLevels;
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