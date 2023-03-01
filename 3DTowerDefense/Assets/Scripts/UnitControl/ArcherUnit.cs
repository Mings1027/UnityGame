using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        protected override void Attack()
        {
            SpawnArrow(transform, TargetPos);
            transform.rotation = Look(transform, TargetPos);
        }

        private void SpawnArrow(Transform startPos, Transform endPos)
        {
            StackObjectPool.Get<Projectile>("ArcherArrow", startPos.position, Quaternion.Euler(-90, 0, 0))
                .Parabola(startPos, endPos.position + endPos.forward * 2).Forget();
        }

        private Quaternion Look(Transform start, Transform direction)
        {
            var dir = direction.position + direction.forward * 2 - start.position;
            start.forward = dir.normalized;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, yRot, 0);
        }
    }
}