using System;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private Rigidbody _rigid;
        protected bool isMoving;

        public event Action<Unit> onDeadEvent;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float attackRange;

        protected override void Awake()
        {
            base.Awake();
            _rigid = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (IsTargeting)
            {
                if (attackAble)
                {
                    if (Vector3.Distance(transform.position, Target.position) <= attackRange)
                    {
                        Attack();
                        StartCoolDown().Forget();
                    }
                }

                _rigid.MovePosition(_rigid.position + Target.position * (Time.fixedDeltaTime * moveSpeed));
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
            _rigid.MovePosition(pos);
        }
    }
}