namespace WeaponControl
{
    public class GunWeapon : Weapon
    {
        protected override void Awake()
        {
            base.Awake();
            bulletType = BulletNameCollection.GunBullet;
        }
    }
}