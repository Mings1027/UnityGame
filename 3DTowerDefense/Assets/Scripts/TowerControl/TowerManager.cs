using UnityEngine;

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
            public float attackRange;
            public Mesh consMesh;
            public Mesh towerMesh;
            public Mesh childMesh;
            public string towerName;
            public string towerInfo;

            public bool IsCoolingDown => Time.time < nextFireTime;
            public void StartCoolDown() => nextFireTime = Time.time + attackCoolDown;
        }
    }
}