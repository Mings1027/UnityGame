using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Arrow : Projectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            if (!other.CompareTag(tagName)) return;
            GetDamage(other);
            var position = transform.position;
            StackObjectPool.Get("ArrowEffect", position);
            StackObjectPool.Get("ArrowExplosionSound", position);
        }
    }
}