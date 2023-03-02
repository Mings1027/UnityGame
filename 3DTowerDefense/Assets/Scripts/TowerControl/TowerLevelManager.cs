using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;

namespace TowerControl
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
            public float constructionTime;
            public string towerName;
            public string towerInfo;
            public int health;
            public int damage;
            public float attackDelay;
            public float attackRange;
        }
    }
}