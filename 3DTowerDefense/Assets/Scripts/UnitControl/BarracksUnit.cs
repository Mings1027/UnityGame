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
        private Transform attackEffectPos;

        public bool isMoving;
        public Vector3 point;
        public event Action onDeadEvent;

        protected override void Awake()
        {
            base.Awake();
            _nav = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            attackEffectPos = transform.GetChild(0);
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(MoveToMousePosition), 0f, 0.5f);
        }

        private void OnDisable()
        {
            onDeadEvent?.Invoke();
            onDeadEvent = null;
            if (IsInvoking())
            {
                CancelInvoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                StackObjectPool.Get("SwordEffect", other.transform.position);
            }
        }

        protected override void CheckState()
        {
            if (!targetFinder.IsTargeting) return;
            if (targetFinder.attackAble &&
                Vector3.Distance(transform.position, targetFinder.Target.position) <= _nav.stoppingDistance)
            {
                Attack();
                targetFinder.StartCoolDown().Forget();
            }
            else
            {
                print("move to target");
                _nav.SetDestination(targetFinder.Target.position);
            }
        }

        protected override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            StackObjectPool.Get("SwordSlashEffect", attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            //이 줄에 Slash 소리 스폰해야함
            if (targetFinder.Target.TryGetComponent(out Health h))
            {
                h.GetHit(targetFinder.Damage, targetFinder.Target.gameObject);
            }
        }

        private void MoveToMousePosition()
        {
            if (!isMoving) return;
            print("move to point");
            _nav.SetDestination(point);
            if (_nav.remainingDistance <= _nav.stoppingDistance) isMoving = false;
        }
    }
}