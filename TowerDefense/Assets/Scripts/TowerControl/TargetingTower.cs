using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _attackSound;
        private Collider[] _targetColliders;
        private bool _isAttack;
        private sbyte _effectIndex;
        private ushort _damage;
        private Cooldown _atkCooldown;

        protected Transform target;
        protected bool isTargeting;
        protected Transform firePos;

        [SerializeField] private LayerMask targetLayer;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }
#endif
        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        protected override void Init()
        {
            base.Init();
            _attackSound = GetComponent<AudioSource>();
            _effectIndex = -1;
            _targetColliders = new Collider[3];
        }

        public override void TowerTargetInit()
        {
            target = null;
            isTargeting = false;
        }

        public override void TowerTargeting()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestTarget = _targetColliders[i].transform;
            }

            target = nearestTarget;
            isTargeting = true;
        }

        public override void TowerUpdate()
        {
            if (!isTargeting || _atkCooldown.IsCoolingDown) return;
            Attack();
            _attackSound.Play();
            _atkCooldown.StartCooldown();
        }

        protected virtual void Attack()
        {
            ProjectileInit();
        }

        private void ProjectileInit()
        {
            var projectile = PoolObjectManager.Get<Projectile>(TowerData.PoolObjectKey, firePos);
            projectile.ColorInit(_effectIndex);
            projectile.Init(_damage, target);
        }

        public override void TowerSetting(MeshFilter towerMesh, ushort damageData, byte rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            _damage = damageData;

            if (TowerLevel % 2 == 0)
            {
                _effectIndex++;
            }

            _atkCooldown.cooldownTime = attackDelayData;
        }
    }
}