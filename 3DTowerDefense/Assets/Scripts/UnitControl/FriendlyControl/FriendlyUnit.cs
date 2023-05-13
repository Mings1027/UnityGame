using System;
using DG.Tweening;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private Vector3 _mousePos;

        protected bool isMoving;

        public event Action<Unit> onDeadEvent;

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
            if (attackAble && Vector3.Distance(transform.position, Target.position) <= nav.stoppingDistance)
            {
                Attack();
                StartCoolDown();
            }
            else
            {
                nav.SetDestination(Target.position);
            }
        }

        protected override void OnDisable()
        {
            onDeadEvent?.Invoke(this);
            onDeadEvent = null;
        }

        public void GoToTargetPosition(Vector3 pos)
        {
            isMoving = true;
            _mousePos = pos;
            nav.SetDestination(_mousePos);
        }

    }
}