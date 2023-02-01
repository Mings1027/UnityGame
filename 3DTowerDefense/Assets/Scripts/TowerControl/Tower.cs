using UnityEngine;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour
    {
        public bool built;
        
        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;
        [SerializeField] private Transform target;

        private void Start()
        {
            targets = new Collider[5];
        }

        public void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, range, targets, enemyLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToEnemy = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = targets[i].transform;
                }
            }

            if (nearestEnemy != null && shortestDistance <= range)
            {
                target = nearestEnemy;
            }
            else
            {
                target = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, range);
            if (target == null) return;
            Gizmos.DrawSphere(target.position, 1);
        }
    }
}