using System;
using AttackControl;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public class BarracksUnit : Unit, IAttackTarget
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;
        private NavMeshAgent _nav;

        public bool movePoint;
        public Vector3 point;
        public event Action onDeadEvent;

        protected override void Awake()
        {
            base.Awake();
            _nav = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke();
            onDeadEvent = null;
        }

        private void Update()
        {
            if (movePoint)
            {
                _nav.SetDestination(point);
                if (_nav.remainingDistance <= _nav.stoppingDistance) movePoint = false;
            }

            if (!IsTargeting) return;
            if (targetFinder.attackAble &&
                Vector3.Distance(transform.position, target.position) <= _nav.stoppingDistance)
            {
                Attack();
            }
            else
            {
                _nav.SetDestination(target.position);
            }
        }

        public void Attack()
        {
            _anim.SetTrigger(IsAttack);
            if (target.TryGetComponent(out Health h))
            {
                h.GetHit(targetFinder.Damage, target.gameObject).Forget();
            }

            targetFinder.StartCoolDown().Forget();
        }
    }
}