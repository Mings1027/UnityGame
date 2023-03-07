using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        private void Update()
        {
            if (!attackAble || !isTargeting) return;
            Attack();
            StartCoolDown().Forget();
        }

        protected override void Attack()
        {
            var t = target.position + target.forward * 2;
            SpawnArrow(t);
            transform.rotation = Look(t);
        }

        private void SpawnArrow(Vector3 endPos)
        {
            var p = StackObjectPool.Get<UnitProjectile>("ArcherUnitArrow", transform.position,
                Quaternion.Euler(-90, 0, 0));
            p.Parabola(transform, endPos).Forget();
            p.damage = damage;
        }

        private Quaternion Look(Vector3 direction)
        {
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, yRot, 0);
        }
    }
}