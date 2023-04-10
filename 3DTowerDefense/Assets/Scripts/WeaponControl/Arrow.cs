using UnityEngine;

namespace WeaponControl
{
    public class Arrow : Projectile
    {
        protected override void HitEffect(Collider col)
        {
            base.HitEffect(col);
            GetDamage(col);
            gameObject.SetActive(false);
        }
    }
}