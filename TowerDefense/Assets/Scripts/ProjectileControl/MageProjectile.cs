using UnitControl.EnemyControl;

namespace ProjectileControl
{
    public sealed class MageProjectile : Projectile
    {
        private byte _decreaseSpeed;
        private byte _slowCoolTime;

        public override void ColorInit(sbyte effectIndex)
        {
            base.ColorInit(effectIndex);
            _decreaseSpeed = (byte)(effectIndex + 1);
            _slowCoolTime = _decreaseSpeed;
        }

        public override void Hit()
        {
            base.Hit();
            TryDamage(target);
            target.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.SlowEffect(_decreaseSpeed, _slowCoolTime);
        }
    }
}