using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        private Transform _target;

        protected override void Awake()
        {
            base.Awake();
            towerName = TowerType.Ballista.ToString();
        }

        protected override void FixedUpdate()
        {
            ParabolaPath(_target.position);
        }

        protected override void ProjectileHit(Collider col)
        {
            ObjectPoolManager.Get(PoolObjectName.BallistaHitSfx, transform.position);
            ApplyDamage(col);
        }

        public void Init(Transform t, int dmg)
        {
            _target = t;
            damage = dmg;
        }
    }
}