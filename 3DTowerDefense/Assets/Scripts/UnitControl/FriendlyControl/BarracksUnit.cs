using System;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl.FriendlyControl
{
    public class BarracksUnit : FriendlyUnit
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;
        private Transform _attackEffectPos;
        private Vector3 _mousePos;
        private bool _isMoving;

        public event Action<BarracksUnit> onDeadEvent;
        [SerializeField] private MeleeWeapon meleeWeapon;

        protected override void Awake()
        {
            base.Awake();
            _anim = GetComponent<Animator>();
            _attackEffectPos = transform.GetChild(0);
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            if (nav.remainingDistance <= nav.stoppingDistance)
            {
                _isMoving = false;
            }
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

        protected override void Attack()
        {
            if (_isMoving) return;

            _anim.SetTrigger(IsAttack);
            StackObjectPool.Get("SwordSlashSFX", transform.position);
            StackObjectPool.Get("SwordSlashEffect", _attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            //이 줄에 Slash 소리 스폰해야함
            meleeWeapon.Attack(target, Damage);
        }

        public void GoToTargetPosition(Vector3 pos)
        {
            _isMoving = true;
            _mousePos = pos;
            nav.SetDestination(_mousePos);
        }
    }
}