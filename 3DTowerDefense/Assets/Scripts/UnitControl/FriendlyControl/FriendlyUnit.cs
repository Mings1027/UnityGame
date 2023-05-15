using System;
using AttackControl;
using DG.Tweening;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private Vector3 _mousePos;

        protected bool isMoving;

        public event Action<Unit> onDeadEvent;

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
            InvokeRepeating(nameof(Targeting), 1, 1);
        }

        private void FixedUpdate()
        {
            if (!isMoving) return;
            if (nav.remainingDistance <= nav.stoppingDistance)
            {
                isMoving = false;
            }
        }

        protected override void Update()
        {
            if (!IsTargeting) return;
            if (attackAble && Vector3.Distance(transform.position, target.position) <= nav.stoppingDistance)
            {
                Attack();
                StartCoolDown();
            }
            else
            {
                nav.SetDestination(target.position);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke(this);
            onDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = IsTargeting ? Color.cyan : Color.red;
            Gizmos.DrawSphere(transform.position + new Vector3(0, 2, 0), 1f);
        }

        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, atkRange, targetColliders, targetLayer);
            IsTargeting = target != null;
        }

        public void GoToTargetPosition(Vector3 pos)
        {
            isMoving = true;
            _mousePos = pos;
            nav.SetDestination(_mousePos);
        }
    }
}