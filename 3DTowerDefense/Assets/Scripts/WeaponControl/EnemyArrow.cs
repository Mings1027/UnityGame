using UnityEngine;

namespace WeaponControl
{
    public class EnemyArrow : Projectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit") || other.CompareTag("Ground"))
            {
                base.OnTriggerEnter(other);
            }
        }
    }
}