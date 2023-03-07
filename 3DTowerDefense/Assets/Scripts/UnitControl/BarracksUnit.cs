using System;
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

        [SerializeField] private float weaponRadius;

        protected override void Awake()
        {
            base.Awake();
            _nav = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
        }
        
        private void Update()
        {
            if (!isTargeting) return;
            _nav.SetDestination(target.position);
            if (Vector3.Distance(transform.position, target.position) > _nav.stoppingDistance) return;
            if (!attackAble) return;
            transform.LookAt(target.position);
            Attack();
            StartCoolDown().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke();
            onDeadEvent = null;
        }

        protected override void Attack()
        {
            _anim.SetTrigger(AttackAble);
            if (target.TryGetComponent(out Health h))
            {
                h.GetHit(damage, gameObject).Forget();
            }
        }
    }
}