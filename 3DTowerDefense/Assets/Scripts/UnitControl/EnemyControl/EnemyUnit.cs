using System;
using GameControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        private Vector3 _destination;
        private Health _health;
        private Rigidbody _rigid;

        public int wayPointIndex;
        public event Action<EnemyUnit> onMoveNextPointEvent;
        public event Action onDeadEvent;
        public event Action onCoinEvent;
        public event Action onLifeCountEvent;

        [SerializeField] private float moveSpeed;

        protected override void Awake()
        {
            base.Awake();
            _health = GetComponent<Health>();
            _rigid = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (IsTargeting)
            {
                if (!attackAble) return;
                Attack();
                StartCoolDown().Forget();
            }
            else
            {
                _rigid.MovePosition( _destination * (Time.fixedDeltaTime * moveSpeed));
                if (Vector3.Distance(transform.position, _destination) <= 0.2f)
                {
                    onMoveNextPointEvent?.Invoke(this);
                }
            }
        }

        public void SetMovePoint(Vector3 pos)
        {
            _destination = pos;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke();
            if (_health.CurHealth > 0) onLifeCountEvent?.Invoke();
            else onCoinEvent?.Invoke();

            onDeadEvent = null;
            onLifeCountEvent = null;
            onCoinEvent = null;
        }
    }
}