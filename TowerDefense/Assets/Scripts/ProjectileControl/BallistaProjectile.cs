using InterfaceControl;
using PoolObjectControl;
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

        protected override void Hit(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            var mainModule = PoolObjectManager.Get<ParticleSystem>(hitParticleKey, transform.position).main;
            mainModule.startColor = towerData.projectileColor[effectIndex];

            damageable.Damage(damage);
        }
    }
}