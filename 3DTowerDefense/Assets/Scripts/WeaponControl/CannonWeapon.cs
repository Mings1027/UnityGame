namespace WeaponControl
{
    public class CannonWeapon : Weapon
    {
        protected override void Awake()
        {
            base.Awake();
            bulletType = BulletNameCollection.CannonBullet;
        }
    }
}
