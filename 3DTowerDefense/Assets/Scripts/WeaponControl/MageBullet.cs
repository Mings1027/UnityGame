using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Projectile
    {
        private Rigidbody rigid;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        protected override void ProjectilePath()
        {
            var position = rigid.position;
            var dir = target.position - position;
            dir *= ProjectileSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(position + dir);
        }

        public override void Init(Transform t, int damage)
        {
            target = t;
            _damage = damage;
        }
    }
}