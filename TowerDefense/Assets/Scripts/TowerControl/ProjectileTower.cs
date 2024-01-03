using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class ProjectileTower : AttackTower
    {
        private Collider[] _targetColliders;
        private TowerState _towerState;
        private LayerMask _targetLayer;

        protected sbyte effectIndex;
        protected bool isTargeting;
        protected Sequence atkSequence;
        protected Collider target;
        protected Transform firePos;
        protected PoolObjectKey projectileKey;

        [SerializeField] protected AudioClip audioClip;
        [SerializeField, Range(1, 5)] private byte targetColliderCount;
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
            _targetLayer = LayerMask.GetMask("Monster") | LayerMask.GetMask("FlyingMonster");
            effectIndex = -1;
            _targetColliders = new Collider[targetColliderCount];
            patrolCooldown.cooldownTime = 0.5f;
            var towerData = (TargetingTowerData)UIManager.Instance.TowerDataPrefabDictionary[TowerType].towerData;
            projectileKey = towerData.PoolObjectKey;
        }

        public override void TowerTargetInit()
        {
            _towerState = TowerState.Detect;
            target = null;
            isTargeting = false;
        }

        public override void TowerUpdate()
        {
            switch (_towerState)
            {
                case TowerState.Detect:
                    Detect();
                    break;
                case TowerState.Attack:
                    ReadyToAttack();
                    break;
            }
        }

        #region Tower State

        protected virtual void Detect()
        {
            if (patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, _targetLayer);

            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _targetColliders[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    target = _targetColliders[i];
                }
            }

            isTargeting = true;
            _towerState = TowerState.Attack;
            patrolCooldown.StartCooldown();
        }

        private void ReadyToAttack()
        {
            if (attackCooldown.IsCoolingDown) return;

            if (!target || !target.enabled || Vector3.Distance(transform.position, target.bounds.center) > TowerRange)
            {
                _towerState = TowerState.Detect;
                return;
            }

            Attack();
            attackCooldown.StartCooldown();
            target = null;
            isTargeting = false;
            _towerState = TowerState.Detect;
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            SoundManager.Instance.Play3DSound(audioClip, transform.position);
            var projectile = PoolObjectManager.Get<Projectile>(projectileKey, firePos.position);
            projectile.ColorInit(effectIndex);
            projectile.Init(Damage, target);
        }

        #endregion

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, rpmData);

            if (TowerLevel % 2 == 0)
            {
                effectIndex++;
            }
        }

        #endregion
    }
}