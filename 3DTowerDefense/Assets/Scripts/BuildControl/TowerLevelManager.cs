using UnityEngine;
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

            [SerializeField] private int minDamage, maxDamage;

            public int Damage => Random.Range(minDamage, maxDamage);
            public float attackDelay;
            public float attackRange;
        }
    }
}