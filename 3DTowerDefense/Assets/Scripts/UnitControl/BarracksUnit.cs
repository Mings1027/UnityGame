using System;
using AttackControl;
using GameControl;
using InterfaceControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public class BarracksUnit : Unit, IFindObject
    {
        private TargetFinder _targetFinder;
        private Animator _anim;

        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public NavMeshAgent _nav;
        public event Action onDeadEvent;

        [SerializeField] private Transform weapon;

        [SerializeField] private float weaponRadius;

        private void Awake()
        {
            _targetFinder = GetComponent<TargetFinder>();
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
            if (!isTargeting) return;
            // _nav.SetDestination(target.position);
            if (!attackAble) return;
            Attack();
            StartCoolDown().Forget();
        }

        // public void MoveToTarget(Vector3 t)
        // {
        //     _nav.SetDestination(t);
        // }

        public override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            if (target.TryGetComponent(out Health h))
            {
                h.GetHit(damage, target.gameObject).Forget();
            }
        }


        public void FindTarget()
        {
            var t = _targetFinder.FindClosestTarget();
            target = t.Item1;
            isTargeting = t.Item2;
        }
    }
}