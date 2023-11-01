namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        public override void Hit()
        {
            base.Hit();
            TryDamage(target);
        }
    }
}