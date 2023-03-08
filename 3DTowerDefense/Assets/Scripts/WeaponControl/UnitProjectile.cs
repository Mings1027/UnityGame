using UnityEngine;

namespace WeaponControl
{
    public class UnitProjectile : Projectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Ground"))
            {
                base.OnTriggerEnter(other);
            }
        }
    }
}