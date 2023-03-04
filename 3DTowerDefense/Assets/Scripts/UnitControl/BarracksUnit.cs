using System;
using System.Collections.Generic;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public class BarracksUnit : Unit
    {
        private NavMeshAgent _nav;
        private Animator _anim;
        private static readonly int AttackAble = Animator.StringToHash("AttackAble");
    
        public event Action onDeadEvent;

        [SerializeField] private Transform weapon;

        [SerializeField] private float atkRadius;
        private void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!IsTargeting) return;
            _nav.SetDestination(Target.position);
            if (Vector3.Distance(transform.position, Target.position) > _nav.stoppingDistance) return;
            if (!attackAble) return;
            transform.LookAt(Target.position);
            Attack();
            StartCoolDown().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke();
            onDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(weapon.position, atkRadius);
        }

        protected override void Attack()
        {
            _anim.SetTrigger(AttackAble);
            if (Target.TryGetComponent(out Health h))
            {
                h.GetHit(damage, gameObject).Forget();
            }
        }

        public void ChangeAnimator(RuntimeAnimatorController animatorController)
        {
            _anim.runtimeAnimatorController = animatorController;
        }
    }
}