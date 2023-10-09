using UnitControl.EnemyControl;

namespace ProjectileControl
{
    public sealed class MageProjectile : Projectile
    {
        private byte decreaseSpeed;
        private byte slowCoolTime;

        public override void ColorInit(sbyte effectIndex)
        {
            base.ColorInit(effectIndex);
            decreaseSpeed = (byte)(effectIndex + 1);
            slowCoolTime = decreaseSpeed;
        }

        public override void Hit()
        {
            base.Hit();
            target.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.SlowEffect(decreaseSpeed, slowCoolTime);
        }
    }
}