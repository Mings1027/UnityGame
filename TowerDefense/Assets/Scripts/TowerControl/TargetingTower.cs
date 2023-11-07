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
        private Collider[] _targetColliders;
        private bool _isAttack;
        private sbyte _effectIndex;
        private int _damage;
        private float _attackDelay;
        private bool _isAttacking;

        protected bool IsTargeting;
        protected Collider Target;
        protected Transform FirePos;

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

        private void ProjectileInit()
        {
            var projectile =
                PoolObjectManager.Get<Projectile>(TowerData.PoolObjectKey, FirePos.position, Quaternion.identity);
            projectile.ColorInit(_effectIndex);
            projectile.Init(_damage, Target);
        }
        
        protected virtual void Attack()
        {
            ProjectileInit();
        }

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
            Target = null;
            IsTargeting = false;
            _isAttacking = false;
        }

        public override void TowerTargeting()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, targetLayer);
            if (size <= 0)
            {
                Target = null;
                IsTargeting = false;
                return;
            }

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

            Target = nearestTarget;
            IsTargeting = true;
        }

        public override async UniTaskVoid TowerAttackAsync(CancellationTokenSource cts)
        {
            if (!IsTargeting || _isAttacking) return;
            _isAttacking = true;
            _attackSound.Play();
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay), cancellationToken: cts.Token);
            _isAttacking = false;
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