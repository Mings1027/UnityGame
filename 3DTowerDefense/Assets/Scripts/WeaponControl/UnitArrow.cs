using UnityEngine;

namespace WeaponControl
{
    public class UnitArrow : Projectile
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