using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _attackSound;

        private bool _isAttack;
        private sbyte _effectIndex;
        private int _damage;
        private float _attackDelay;
        private bool _isAttacking;
        private Collider[] _targetColliders;

        protected bool isTargeting;
        protected Collider target;
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


        #region Override Function

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
            _isAttacking = false;
        }

        public override void TowerUpdate(CancellationTokenSource cts)
        {
            Patrol();
            if (!isTargeting || _isAttacking) return;
            AttackAsync(cts).Forget();
        }

        private void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, targetLayer);
            if (size <= 0) return;

            var shortestDistance = Mathf.Infinity;
            Collider nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestTarget = _targetColliders[i];
            }

            target = nearestTarget;
            isTargeting = target;
        }

        private async UniTaskVoid AttackAsync(CancellationTokenSource cts)
        {
            _isAttacking = true;
            _attackSound.Play();
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        private void ProjectileInit()
        {
            var projectile =
                PoolObjectManager.Get<Projectile>(TowerData.PoolObjectKey, firePos.position, Quaternion.identity);
          
            projectile.ColorInit(_effectIndex);
            projectile.Init(_damage, target);
        }

        protected virtual void Attack()
        {
            ProjectileInit();
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            _damage = damageData;

            if (TowerLevel % 2 == 0)
            {
                _effectIndex++;
            }

            _attackDelay = attackDelayData;
        }

        #endregion
    }
}