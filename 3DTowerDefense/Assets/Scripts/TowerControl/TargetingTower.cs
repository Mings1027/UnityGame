using System;
using DG.Tweening;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        [Serializable]
        public struct TowerStatus
        {
            public float towerRange;
            public int minDamage;
            public int maxDamage;

            public TowerStatus(float towerRange, int minDamage, int maxDamage)
            {
                this.towerRange = towerRange;
                this.minDamage = minDamage;
                this.maxDamage = maxDamage;
            }
        }

        public TowerStatus TowerStat { get; private set; }

        protected int Damage => Random.Range(TowerStat.minDamage, TowerStat.maxDamage);
        protected Transform target;
        protected bool isTargeting;
        protected Action onAttackEvent;
        protected Collider[] targetColliders;

        private Tween _delayTween;
        private bool _attackAble;

        [SerializeField] private LayerMask targetLayer;

        protected override void Awake()
        {
            base.Awake();
            onAttackEvent = () => { };
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attackAble = true;
            InvokeRepeating(nameof(Targeting), 1, 0.5f);
        }

        private void Update()
        {
            if (isUpgrading || !_attackAble || !isTargeting) return;
            Attack();
            StartCoolDown();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            _delayTween?.Kill();
        }

        protected virtual void Attack()
        {
            onAttackEvent.Invoke();
        }

        private void Targeting()
        {
            var t = SearchTarget.ClosestTarget(transform.position, TowerStat.towerRange, targetColliders,
                targetLayer);
            target = t.Item1;
            isTargeting = t.Item2;
        }

        private void StartCoolDown()
        {
            _attackAble = false;
            _delayTween.Restart();
        }

        public override void BuildTowerDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay, float health = 0)
        {
            base.BuildTowerDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            TowerStat = new TowerStatus(attackRange, minDamage, maxDamage);

            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(attackDelay, () => _attackAble = true, false).SetAutoKill(false);
        }
    }
}