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
        private sbyte _effectIndex;
        private bool _isAttacking;

        protected Collider[] targetColliders;
        protected AudioSource attackSound;
        protected TowerState towerState;
        protected LayerMask targetLayer;
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
            targetLayer = LayerMask.GetMask("Monster");
            attackSound = GetComponent<AudioSource>();
            _effectIndex = -1;
            targetColliders = new Collider[3];
        }

        public override void TowerTargetInit()
        {
            towerState = TowerState.Patrol;
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
            if (towerState == TowerState.Attack) AttackAsync(cts).Forget();
        }

        protected virtual void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
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
                    Vector3.SqrMagnitude(transform.position - targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                target = targetColliders[i];
            }

            isTargeting = true;
            towerState = TowerState.Attack;
        }

        private async UniTaskVoid AttackAsync(CancellationTokenSource cts)
        {
            if (_isAttacking)
            {
                towerState = TowerState.Patrol;
                return;
            }

            _isAttacking = true;
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(AttackDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            attackSound.Play();
            var targetingTowerData = (TargetingTowerData)TowerData;
            var projectile =
                PoolObjectManager.Get<Projectile>(targetingTowerData.PoolObjectKey, firePos.position,
                    Quaternion.identity);

            projectile.ColorInit(_effectIndex);
            projectile.Init(Damage, target);
            projectile.ProjectileUpdate().Forget();
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