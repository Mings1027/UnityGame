using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Vector3 _destination;
        private int _wayPointIndex;
        private bool _isSpeedDeBuffed;
        private static readonly int Walk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform Target { get; set; }
        public bool IsTargeting { get; set; }

        public event Action<int, EnemyUnit> OnMoveNextPointEvent;

        [SerializeField] private float moveSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();
            _wayPointIndex = 0;
            Target = null;
            IsTargeting = false;
            InvokeRepeating(nameof(CheckTarget), 1, 1);
        }

        private void FixedUpdate()
        {
            if (IsTargeting)
            {
                anim.SetBool(Walk, false);
                if (!isCoolingDown) return;

                anim.SetTrigger(IsAttack);
                Attack();
                StartCoolDown().Forget();
            }
            else
            {
                var position = rigid.position;
                var dir = (_destination - position).normalized;
                var moveVec = dir * (moveSpeed * Time.deltaTime);
                anim.SetBool(Walk, true);
                rigid.MovePosition(position + moveVec);
                if (Vector3.Distance(transform.position, _destination) <= 1)
                {
                    OnMoveNextPointEvent?.Invoke(_wayPointIndex, this);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnMoveNextPointEvent = null;
            CancelInvoke();
        }

        public void SetMovePoint(Vector3 pos)
        {
            _wayPointIndex++;
            _destination = pos;
        }

        public async UniTaskVoid SlowMovement(float deBuffTime, float decreaseSpeed)
        {
            if (_isSpeedDeBuffed) return;
            _isSpeedDeBuffed = true;
            moveSpeed -= decreaseSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(deBuffTime), cancellationToken: cts.Token);
            moveSpeed += decreaseSpeed;
            _isSpeedDeBuffed = false;
        }

        private void CheckTarget()
        {
            if (Target == null) return;
            if (Target.gameObject.activeSelf) return;
            Target = null;
            IsTargeting = false;
        }
    }
}