using UnityEngine;

namespace TowerControl
{
    [CreateAssetMenu]
    public class TowerLevelManager : ScriptableObject
    {
        [SerializeField] private GameObject towerPrefab;
        public TowerLevel[] towerLevels;

        [System.Serializable]
        public class TowerLevel
        {
            public float constructionTime;
            public Mesh consMesh;
            public Mesh towerMesh;
            public string towerName;
            public string towerInfo;
            
        }
    }
}