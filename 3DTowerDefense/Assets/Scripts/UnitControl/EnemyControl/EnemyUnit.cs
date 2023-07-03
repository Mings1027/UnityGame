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

        public Transform Target { get; set; }
        public bool IsTargeting { get; set; }
        public event Action<int, EnemyUnit> onMoveNextPointEvent;

        [SerializeField] [Range(0, 1)] private float turnSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();
            _wayPointIndex = 0;
            Target = null;
            IsTargeting = false;
        }

        private void FixedUpdate()
        {
            if (IsTargeting)
            {
                if (!attackAble) return;

                nav.isStopped = true;
                Attack();
                StartCoolDown().Forget();
            }
            else
            {
                nav.isStopped = false;
                nav.SetDestination(_destination);
                if (Vector3.Distance(transform.position, _destination) <= nav.stoppingDistance)
                {
                    onMoveNextPointEvent?.Invoke(_wayPointIndex, this);
                }
            }
        }

        private void LateUpdate()
        {
            if (!IsTargeting) return;
            LookTarget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onMoveNextPointEvent = null;
        }

        public void SetMovePoint(Vector3 pos)
        {
            _wayPointIndex++;
            _destination = pos;
        }

        private void LookTarget()
        {
            var dir = (Target.position - transform.position).normalized;
            var lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed);
        }

        public async UniTaskVoid SlowMovement(float deBuffTime, float decreaseSpeed)
        {
            if (_isSpeedDeBuffed) return;
            _isSpeedDeBuffed = true;
            nav.speed -= decreaseSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(deBuffTime), cancellationToken: cts.Token);
            nav.speed += decreaseSpeed;
            _isSpeedDeBuffed = false;
        }
    }
}