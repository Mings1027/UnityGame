using DataControl;
using GameControl;
using InterfaceControl;
using ManagerControl;
using SoundControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private Vector3 _targetEndPos;
        private Collider[] _targetColliders;

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
            ParabolaPath(_targetEndPos);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void TryHit()
        {
            var pos = transform.position;

            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);

            if (size <= 0) return;
            
            ObjectPoolManager.Get<SoundPlayer>(StringManager.CanonHitVfx, pos).Play();

            for (var i = 0; i < size; i++)
            {
                ObjectPoolManager.Get(StringManager.BloodVfx, _targetColliders[i].transform.position);
                if (_targetColliders[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(damage);
                    DataManager.SumDamage(towerName, damage);
                }
            }
        }


        public void Init(Vector3 t, int dmg)
        {
            Physics.Raycast(t + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
            damage = dmg;
        }
    }
}