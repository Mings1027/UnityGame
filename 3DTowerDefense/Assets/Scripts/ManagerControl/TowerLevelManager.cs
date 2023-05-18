using UnityEngine;

namespace ManagerControl
{
    [CreateAssetMenu]
    public class TowerLevelManager : ScriptableObject
    {
        public TowerLevel[] towerLevels;

        [System.Serializable]
        public class TowerLevel
        {
            public MeshFilter towerMesh;
            public MeshFilter consMesh;
            public string towerName;
            public string towerInfo;
            public int minDamage, maxDamage;
            public float attackRange;
            public float attackDelay;
            public int health;
        }
    }
}