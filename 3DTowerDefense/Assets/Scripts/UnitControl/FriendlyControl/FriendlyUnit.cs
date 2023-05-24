using System;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
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
            if (IsTargeting)
            {
                if (attackAble)
                {
                    if (Vector3.Distance(transform.position, Target.position) <= nav.stoppingDistance)
                    {
                        Attack();
                        StartCoolDown().Forget();
                    }
                    else
                    {
                        nav.SetDestination(Target.position);
                    }
                }
                else
                {
                    nav.SetDestination(Target.position);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke(this);
            onDeadEvent = null;
        }

        public void GoToTargetPosition(Vector3 pos)
        {
            isMoving = true;
            nav.SetDestination(pos);
        }
    }
}