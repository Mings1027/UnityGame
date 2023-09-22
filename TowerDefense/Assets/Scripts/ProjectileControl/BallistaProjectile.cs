using ManagerControl;

namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        protected override void Awake()
        {
            base.Awake();
            towerType = TowerType.Ballista;
        }
    }
}