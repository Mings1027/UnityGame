using System;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public class BarracksUnit : Unit
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;
        private NavMeshAgent _nav;

        public bool isMoving;
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
        }

        protected override void CheckState()
        {
            if (isMoving)
            {
                _nav.SetDestination(point);
                if (_nav.remainingDistance <= _nav.stoppingDistance) isMoving = false;
            }

            if (!targetFinder.IsTargeting) return;
            if (targetFinder.attackAble &&
                Vector3.Distance(transform.position, targetFinder.Target.position) <= _nav.stoppingDistance)
            {
                Attack();
                targetFinder.StartCoolDown();
            }
            else
            {
                _nav.SetDestination(targetFinder.Target.position);
            }
        }

        protected override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            if (targetFinder.Target.TryGetComponent(out Health h))
            {
                h.GetHit(targetFinder.Damage, targetFinder.Target.gameObject);
            }
        }
    }
}