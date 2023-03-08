using System;
using TowerControl;
using UnityEngine;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour
    {
        private Vector3 _checkRangePoint;
        private Collider[] _results;

        public float range;

        [SerializeField] private LayerMask targetLayer;

        private void Awake()
        {
            _results = new Collider[3];
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        public (Transform, bool) FindClosestTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, range, _results, targetLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            if (size <= 0) return (null, false);

            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _results[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _results[i].transform;
            }

            return (nearestEnemy, true);
        }
    }
}