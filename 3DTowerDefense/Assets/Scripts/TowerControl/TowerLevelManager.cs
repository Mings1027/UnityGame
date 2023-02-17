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
            public float constructionTime;
            public Mesh consMesh;
            public Mesh towerMesh;
            public Mesh childMesh;
            public string towerName;
            public string towerInfo;
            
        }
    }
}