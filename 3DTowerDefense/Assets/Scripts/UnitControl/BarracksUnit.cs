using System;
using System.Collections.Generic;
using AttackControl;
using GameControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

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
            atkRange = 2;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(FindObj), 0, 0.5f);
        }

        private void Update()
        {
            if (!IsTargeting) return;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkRangePoint, atkRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(weapon.position, weaponRadius);
        }

        protected override void Attack()
        {
            _anim.SetTrigger(AttackAble);
            if (target.TryGetComponent(out Health h))
            {
                h.GetHit(damage, gameObject).Forget();
            }
        }

        private void FindObj()
        {
            var t = ObjectFinder.FindClosestObject(transform.position, atkRange, hitCollider, EnemyLayer);
            target = t.Item1;
            IsTargeting = t.Item2;
        }
    }
}