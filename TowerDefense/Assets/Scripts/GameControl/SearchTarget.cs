using UnityEngine;

namespace GameControl
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

        public static void SortTargetByDistance(Vector3 pos, Collider[] targetColliders)
        {
            for (var i = 0; i < targetColliders.Length - 1; i++)
            {
                var curTarget = targetColliders[i];
                var curDistance = Vector3.SqrMagnitude(pos - curTarget.transform.position);
                var j = i - 1;
                while (j >= 0 && Vector3.SqrMagnitude(pos - targetColliders[j].transform.position) > curDistance)
                {
                    targetColliders[j + 1] = targetColliders[j];
                    j--;
                }

                targetColliders[j + 1] = curTarget;
            }
        }
    }
}