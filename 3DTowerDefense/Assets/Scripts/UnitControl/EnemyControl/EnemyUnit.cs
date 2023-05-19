using System;
using AttackControl;
using DG.Tweening;
using ManagerControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Transform destination;

        public event Action onWaveEndedEvent;

        [SerializeField] private int atkRange;
        [SerializeField] private LayerMask targetLayer;

        protected override void Awake()
        {
            base.Awake();
            targetColliders = new Collider[3];
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
                if (Vector3.Distance(transform.position, target.position) <= atkRange)
                {
                    if (!attackAble) return;
                    nav.isStopped = true;
                    Attack();
                    StartCoolDown();
                }
                else
                {
                    nav.SetDestination(target.position);
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
            base.OnDisable();
            onWaveEndedEvent?.Invoke();
            onWaveEndedEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public void SetDestination(Transform pos)
        {
            destination = pos;
        }
        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, atkRange, targetColliders, targetLayer);
            IsTargeting = target != null;
        }
    }
}