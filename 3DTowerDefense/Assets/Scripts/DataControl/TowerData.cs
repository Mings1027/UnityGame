using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class TowerData : ScriptableObject
    {
        public TowerLevelData[] towerLevels;
        public TowerLevelData[] towerUniqueLevels;

        [System.Serializable]
        public class TowerLevelData
        {
            public MeshFilter towerMesh;
            public MeshFilter consMesh;
            public string towerName;
            public string towerInfo;
            public int minDamage, maxDamage;
            public float attackRange;
            public float attackDelay;
            public float health;
        }
    }
}