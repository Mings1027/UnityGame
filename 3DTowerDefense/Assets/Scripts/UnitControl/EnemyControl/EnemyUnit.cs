using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Vector3 _destination;
        private int _wayPointIndex;
        private bool _isMove;
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
        }

        private void Update()
        {
            if (IsTargeting)
            {
                DoAttack();
            }
            else
            {
                MoveWayPoint();
            }
        }

        private void LateUpdate()
        {
            anim.SetBool(Walk, _isMove);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnMoveNextPointEvent = null;
        }

        private void DoAttack()
        {
            if (!isCoolingDown) return;
            if (!Target.gameObject.activeSelf)
            {
                Target = null;
                IsTargeting = false;
                return;
            }

            _isMove = false;

            anim.SetTrigger(IsAttack);
            Attack();
            StartCoolDown().Forget();
        }

        private void MoveWayPoint()
        {
            _isMove = true;
            var position = rigid.position;
            var dir = (_destination - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            rigid.MovePosition(position + moveVec);
            if (Vector3.Distance(transform.position, _destination) <= 1)
            {
                OnMoveNextPointEvent?.Invoke(_wayPointIndex, this);
            }
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
    }
}