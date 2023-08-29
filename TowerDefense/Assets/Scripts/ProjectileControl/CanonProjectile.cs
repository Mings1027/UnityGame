using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private Vector3 _targetEndPos;
        private Collider[] _targetColliders;

        [SerializeField] private Transform effects;
        [SerializeField] private ParticleSystem explosionParticle;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
            towerName = TowerType.Canon.ToString();
        }

        protected override void FixedUpdate()
        {
            if (isArrived) return;
            ParabolaPath(_targetEndPos);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            for (var i = 0; i < effects.childCount; i++)
            {
                effects.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            explosionParticle.Play();
            ObjectPoolManager.Get(PoolObjectName.CanonHitSfx, pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                ApplyDamage(_targetColliders[i]);
            }
        }

        public void Init(Vector3 t, int dmg, int effectCount)
        {
            Physics.Raycast(t + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
            damage = dmg;

            for (var i = 0; i < effectCount; i++)
            {
                effects.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}