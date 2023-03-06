using UnityEngine;

namespace AttackControl
{
    public abstract class ObjectFinder
    {
        public static (Transform, bool) FindClosestObject(Vector3 pos, float radius, Collider[] results,
            LayerMask layerMask)
        {
            var size = Physics.OverlapSphereNonAlloc(pos, radius, results, layerMask);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            if (size <= 0) return (null, false);
            for (var i = 0; i < size; i++)
            {
                var r = results[i];
                var distanceToResult = Vector3.SqrMagnitude(pos - r.transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = r.transform;
            }

            return (nearestEnemy, true);
        }

        public static RaycastHit HitObject(Vector3 pos, Vector3 dir, LayerMask layerMask)
        {
            return Physics.Raycast(pos, dir, out var hit, 100, layerMask) ? hit : default;
        }
    }
}