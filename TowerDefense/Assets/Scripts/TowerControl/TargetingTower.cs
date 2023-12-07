using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _attackSound;
        private sbyte _effectIndex;
        private bool _isAttacking;
        private Collider[] _targetColliders;
        private TowerState _towerState;
        private LayerMask _targetLayer;

        protected bool isTargeting;
        protected Sequence atkSequence;
        protected Collider target;
        protected Transform firePos;


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }
#endif
        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        #region Unity Event

        private void OnDestroy()
        {
            atkSequence?.Kill();
        }

        #endregion

        #region Override Function

        protected override void Init()
        {
            base.Init();
            _targetLayer = LayerMask.GetMask("Monster");
            _attackSound = GetComponent<AudioSource>();
            _effectIndex = -1;
            _targetColliders = new Collider[3];
        }

        public override void TowerTargetInit()
        {
            _towerState = TowerState.Patrol;
            target = null;
            isTargeting = false;
            _isAttacking = false;
        }

        public override void TowerTargeting()
        {
            Patrol();
        }

        public override void TowerUpdate(CancellationTokenSource cts)
        {
            if (_towerState == TowerState.Attack) AttackAsync(cts).Forget();
        }

        private void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, _targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = Mathf.Infinity;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                target = _targetColliders[i];
            }

            isTargeting = true;
            _towerState = TowerState.Attack;
        }

        private async UniTaskVoid AttackAsync(CancellationTokenSource cts)
        {
            if (_isAttacking)
            {
                _towerState = TowerState.Patrol;
                return;
            }

            _isAttacking = true;
            _attackSound.Play();
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(AttackDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            var targetingTowerData = (TargetingTowerData)TowerData;
            var projectile =
                PoolObjectManager.Get<Projectile>(targetingTowerData.PoolObjectKey, firePos.position,
                    Quaternion.identity);

            projectile.ColorInit(_effectIndex);
            projectile.Init(Damage, target);
            projectile.ProjectileUpdate().Forget();
            _towerState = TowerState.Patrol;
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, rpmData);

            if (TowerLevel % 2 == 0)
            {
                _effectIndex++;
            }
        }

        #endregion
    }
}