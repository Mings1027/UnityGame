using System;
using CustomEnumControl;
using InterfaceControl;
using MonsterControl;
using PoolObjectControl;
using UnityEngine;

namespace ProjectileControl
{
    public sealed class WizardProjectile : Projectile
    {
        private byte _decreaseSpeed;
        private byte _slowCoolTime;

        public void DeBuffInit(sbyte vfxIndex)
        {
            _decreaseSpeed = (byte)Math.Pow(2, vfxIndex + 1); //2 4 8
            _slowCoolTime = _decreaseSpeed;
        }

        protected override void ProjectilePath(Vector3 endPos)
        {
            base.ProjectilePath(endPos);
            rigid.MovePosition(curPos);
        }

        protected override void Hit(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            var mainModule = PoolObjectManager.Get<ParticleSystem>(hitParticleKey, transform.position).main;
            mainModule.startColor = towerData.projectileColor[effectIndex];

            target.TryGetComponent(out MonsterStatus enemyStatus);
            enemyStatus.SlowEffect(_decreaseSpeed, _slowCoolTime);

            damageable.Damage(damage);
        }
    }
}