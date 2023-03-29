using System;
using GameControl;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public class BarracksUnit : FriendlyUnit
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;
        private Transform _attackEffectPos;

        public bool isMoving;
        public Vector3 point;
        public event Action<BarracksUnit> onDeadEvent;

        protected override void Awake()
        {
            base.Awake();
            _anim = GetComponent<Animator>();
            _attackEffectPos = transform.GetChild(0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(MoveToMousePosition), 0f, 0.5f);
        }

        private void OnDisable()
        {
            onDeadEvent?.Invoke(this);
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

        protected override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            StackObjectPool.Get("SwordSlashEffect", _attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            //이 줄에 Slash 소리 스폰해야함
            if (target.TryGetComponent(out Health h))
            {
                h.GetHit(Damage, target.gameObject);
            }
        }

        private void MoveToMousePosition()
        {
            if (!isMoving) return;
            nav.SetDestination(point);
            if (nav.remainingDistance <= nav.stoppingDistance) isMoving = false;
        }
    }
}