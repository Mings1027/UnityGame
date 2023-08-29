using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        private Transform _target;
        [SerializeField] private Transform effects;

        protected override void Awake()
        {
            base.Awake();
            towerName = TowerType.Ballista.ToString();
        }

        protected override void FixedUpdate()
        {
            if (isArrived) return;
            ParabolaPath(_target.position);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            for (var i = 0; i < effects.childCount; i++)
            {
                effects.GetChild(i).gameObject.SetActive(false);
            }
        }

        protected override void ProjectileHit(Collider col)
        {
            ObjectPoolManager.Get(PoolObjectName.BallistaHitSfx, transform.position);
            ApplyDamage(col);
        }

        public void Init(Transform t, int dmg, int effectCount)
        {
            _target = t;
            damage = dmg;

            for (var i = 0; i < effectCount; i++)
            {
                effects.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}