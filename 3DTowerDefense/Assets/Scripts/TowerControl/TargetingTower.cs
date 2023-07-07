using System;
using DG.Tweening;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Tween _delayTween;
        private bool _attackAble;
        private int _minDamage, _maxDamage;

        public float TowerRange { get; private set; }
        protected LayerMask TargetLayer => targetLayer;
        protected int Damage => Random.Range(_minDamage, _maxDamage);
        protected Transform target;
        protected bool isTargeting;

        protected Action onAttackEvent;
        protected Collider[] targetColliders;

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
            onAttackEvent = null;
            CancelInvoke();
            _delayTween?.Kill();
        }

        protected virtual void Attack()
        {
            onAttackEvent.Invoke();
        }

        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, TowerRange, targetColliders, TargetLayer);
            isTargeting = target != null;
        }

        private void StartCoolDown()
        {
            _attackAble = false;
            _delayTween.Restart();
        }

        public override void TowerInit(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay, float health = 0)
        {
            base.TowerInit(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            TowerRange = attackRange;
            _minDamage = minDamage;
            _maxDamage = maxDamage;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(attackDelay, () => _attackAble = true, false).SetAutoKill(false);
        }
    }
}