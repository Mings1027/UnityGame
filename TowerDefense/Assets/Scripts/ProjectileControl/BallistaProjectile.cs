using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
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
            ParabolaPath(_target.transform.position);
        }

        protected override void TryHit()
        {
            if (_target == null) return;
            ObjectPoolManager.Get(StringManager.BloodVfx, transform.position);
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(damage);
                DataManager.SumDamage(towerName, damage);
            }
        }

        public void Init(Transform t, int dmg)
        {
            _target = t;
            damage = dmg;
        }
    }
}