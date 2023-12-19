using System;
using MonsterControl;

namespace ProjectileControl
{
    public sealed class WizardProjectile : Projectile
    {
        private byte _decreaseSpeed;
        private byte _slowCoolTime;

        public void DeBuffInit(sbyte effectIndex)
        {
            _decreaseSpeed = (byte)Math.Pow(2, effectIndex + 1); //2 4 8
            _slowCoolTime = _decreaseSpeed;
        }

        public override void Hit()
        {
            base.Hit();
            TryDamage(target);
            target.TryGetComponent(out MonsterStatus enemyStatus);
            enemyStatus.SlowEffect(_decreaseSpeed, _slowCoolTime);
        }
    }
}