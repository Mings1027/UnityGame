using GameControl;
using ManagerControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl
{
    public class ArcherUnit : Unit
    {
        protected override void CheckState()
        {
            if (!targetFinder.IsTargeting || !targetFinder.attackAble) return;
            Attack();
            targetFinder.StartCoolDown();
        }

        protected override void Attack()
        {
            SoundManager.PlaySound(SoundManager.Sound.Arrow,transform.position);
            var target = targetFinder.Target;
            var t = target.position + target.forward;
            SpawnArrow(t);
        }

        private void SpawnArrow(Vector3 endPos)
        {
            var p = StackObjectPool.Get<Projectile>("UnitProjectile", transform.position,
                Quaternion.Euler(-90, 0, 0));
            p.Parabola(transform, endPos).Forget();
            p.damage = targetFinder.Damage;
        }
    }
}