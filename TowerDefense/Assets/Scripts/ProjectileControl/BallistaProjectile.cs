using UnityEngine;

namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        protected override void ProjectilePath(Vector3 endPos)
        {
            base.ProjectilePath(endPos);
            var t = transform;
            var dir = (curPos - t.position).normalized;
            if (dir == Vector3.zero) return;
            transform.SetPositionAndRotation(curPos, Quaternion.LookRotation(dir));
        }
    }
}