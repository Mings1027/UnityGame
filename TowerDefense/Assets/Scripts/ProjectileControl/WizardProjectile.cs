using UnitControl.EnemyControl;

namespace ProjectileControl
{
    public sealed class WizardProjectile : Projectile
    {
        private byte _decreaseSpeed;
        private byte _slowCoolTime;

        public override void ColorInit(sbyte effectIndex)
        {
            base.ColorInit(effectIndex);
            _decreaseSpeed = (byte)(effectIndex + 2);   //2 3 4
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