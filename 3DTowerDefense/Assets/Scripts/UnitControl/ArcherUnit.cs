using GameControl;
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
            StackObjectPool.Get("ArrowShootSound", transform.position);
            var target = targetFinder.Target;
            var t = target.position + target.forward;
            SpawnArrow(t);
        }

        private void SpawnArrow(Vector3 endPos)
        {
            var startPos = transform.position;
            var p = StackObjectPool.Get<Projectile>("UnitArrow", startPos);
            p.SetPosition(endPos);
            p.damage = targetFinder.Damage;
        }
    }
}