using System;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class TowerData : ScriptableObject
    {
        public TowerLevelData[] towerLevels;
        public string towerName;

        [Serializable]
        public class TowerLevelData
        {
            public MeshFilter towerMesh;
            public int damage;
            public int attackRange;
            public float attackDelay;
        }
    }
}