using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        protected override void Attack()
        {
            SpawnArrow(transform, targetPos);
            transform.rotation = Look(transform, targetPos);
        }

        private void SpawnArrow(Transform t, Vector3 direction)
        {
            StackObjectPool.Get<Projectile>("ArcherArrow", t.position, Quaternion.Euler(-90, 0, 0))
                .Parabola(t, direction).Forget();
        }

        private Quaternion Look(Transform start, Vector3 direction)
        {
            var dir = direction - start.position;
            start.forward = dir.normalized;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, yRot, 0);
        }
    }
}