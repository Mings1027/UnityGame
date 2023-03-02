using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        private void Update()
        {
            if (!attackAble || !IsTargeting) return;
            Attack();
            StartCoolDown().Forget();
        }

        protected override void Attack()
        {
            var t = Target.position + Target.forward * 2;
            SpawnArrow(t);
            transform.rotation = Look(t);
        }   

        private void SpawnArrow(Vector3 endPos)
        {
            StackObjectPool.Get<Projectile>("ArcherArrow", transform.position, Quaternion.Euler(-90, 0, 0))
                .Parabola(transform, endPos).Forget();
        }

        private Quaternion Look(Vector3 direction)
        {
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, yRot, 0);
        }
    }
}