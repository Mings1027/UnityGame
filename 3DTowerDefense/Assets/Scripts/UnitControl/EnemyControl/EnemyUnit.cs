using System;
using GameControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Vector3 _destination;
        private Health _health;
        private int _wayPointIndex;

        public Transform Target { get; set; }
        public bool IsTargeting { get; set; }
        public event Action<int, EnemyUnit> onMoveNextPointEvent;
        public event Action onDeadEvent;
        public event Action onIncreaseCoinEvent;
        public event Action onLifeCountEvent;

        [SerializeField] [Range(0, 1)] private float turnSpeed;

        protected override void Awake()
        {
            base.Awake();
            _health = GetComponent<Health>();
        }

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
            onDeadEvent?.Invoke();
            if (_health.CurHealth > 0) onLifeCountEvent?.Invoke();
            else onIncreaseCoinEvent?.Invoke();

            onMoveNextPointEvent = null;
            onDeadEvent = null;
            onIncreaseCoinEvent = null;
            onLifeCountEvent = null;
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
    }
}