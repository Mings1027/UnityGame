using System;
using AttackControl;
using DG.Tweening;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Collider[] _targetColliders;

        public int Number { get; set; }
        public Transform destination;

        public event Action<int> onFinishWaveCheckEvent;
        [SerializeField] private int atkRange;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[1];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        protected override void Update()
        {
            if (IsTargeting)
            {
                if (Vector3.Distance(transform.position, Target.position) <= atkRange)
                {
                    if (!attackAble) return;
                    nav.isStopped = true;
                    Attack();
                    StartCoolDown();
                }
                else
                {
                    nav.SetDestination(Target.position);
                }
            }
            else
            {
                if (nav.isStopped) nav.isStopped = false;
                nav.SetDestination(destination.position);
                if (Vector3.Distance(transform.position, destination.position) <= nav.stoppingDistance)
                    gameObject.SetActive(false);
            }
        }

        protected override void OnDisable()
        {
            onFinishWaveCheckEvent?.Invoke(Number);
            onFinishWaveCheckEvent = null;
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        private void Targeting()
        {
            var c = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, TargetLayer);
            Target = c.Item1;
            IsTargeting = c.Item2;
        }
    }
}