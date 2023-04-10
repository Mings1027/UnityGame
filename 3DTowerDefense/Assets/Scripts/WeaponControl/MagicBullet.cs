using System;
using UnityEngine;

namespace WeaponControl
{
    public class MagicBullet : Projectile
    {
        private Rigidbody rigid;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        protected override void FixedUpdate()
        {
            var position = rigid.position;
            var dir = Target.position - position;
            dir *= ProjectileSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(position + dir);
        }

        protected override void HitEffect(Collider col)
        {
            base.HitEffect(col);
            GetDamage(col);
            gameObject.SetActive(false);
        }
    }
}