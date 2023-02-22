using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;

namespace TowerControl
{
    [CreateAssetMenu]
    public class TowerManager : ScriptableObject
    {
        public TowerLevel[] towerLevels;
        private static float nextFireTime;

        [System.Serializable]
        public class TowerLevel
        {
            public float constructionTime;
            public float attackCoolDown;
            public MeshFilter towerMesh;
            public MeshFilter consMesh;
            public float attackRange;
            public string towerName;
            public string towerInfo;

            public bool IsCoolingDown => Time.time < nextFireTime;
            public void StartCoolDown() => nextFireTime = Time.time + attackCoolDown;
        }
    }
    
}