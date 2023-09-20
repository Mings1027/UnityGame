using DataControl;
using UnitControl.EnemyControl;

namespace ProjectileControl
{
    public sealed class MageProjectile : Projectile
    {
        public DeBuffData.SpeedDeBuffData speedDeBuffData;

        protected override void TryHit()
        {
            base.TryHit();
            target.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.SlowEffect(ref speedDeBuffData);
        }
    }
}