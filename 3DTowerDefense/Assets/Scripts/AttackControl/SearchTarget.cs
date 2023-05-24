using UnitControl;
using UnityEngine;

namespace AttackControl
{
    public class SearchTarget : MonoBehaviour
    {
        public static Transform ClosestTarget(Vector3 pos, float range, Collider[] targetColliders,
            LayerMask targetLayer)
        {
            var size = Physics.OverlapSphereNonAlloc(pos, range, targetColliders, targetLayer);
            if (size <= 0) return null;

            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;

            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(pos - targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestTarget = targetColliders[i].transform;
            }

            return nearestTarget;
        }
    }
}