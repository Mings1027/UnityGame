using UnityEngine;

namespace AttackControl
{
    public class SearchTarget : MonoBehaviour
    {
        private static GameObject targetObj;

        public static (Transform, bool) ClosestTarget(Vector3 pos, float range, Collider[] targetColliders,
            LayerMask targetLayer)
        {
            var size = Physics.OverlapSphereNonAlloc(pos, range, targetColliders, targetLayer);
            if (size <= 0) return (null, false);

            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            for (var i = 0; i < size; i++)
            {
                if (targetColliders[i].gameObject == targetObj) continue;
                var distanceToResult =
                    Vector3.SqrMagnitude(pos - targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = targetColliders[i].transform;
            }

            targetObj = nearestEnemy != null ? nearestEnemy.gameObject : null;

            return (nearestEnemy, nearestEnemy != null);
        }
    }
}