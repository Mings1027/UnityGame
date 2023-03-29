using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Arrow : Projectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(hitName))
            {
                var position = transform.position;
                StackObjectPool.Get("ArrowEffect", position);
                StackObjectPool.Get("ArrowExplosionSound", position);
                GetDamage(other);
                gameObject.SetActive(false);
            }
        }
    }
}