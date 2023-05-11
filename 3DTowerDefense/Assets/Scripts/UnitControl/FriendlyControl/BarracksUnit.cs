using System;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl.FriendlyControl
{
    public class BarracksUnit : FriendlyUnit
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private AudioSource audioSource;
        private Animator _anim;
        private Transform _attackEffectPos;
        private Vector3 _mousePos;
        private bool _isMoving;

        public event Action<BarracksUnit> onDeadEvent;
        public bool IsMatching { get; set; }

        [SerializeField] private MeleeWeapon meleeWeapon;
        [SerializeField] private AudioClip atkAudio;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            _anim = GetComponent<Animator>();
            _attackEffectPos = transform.GetChild(0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            IsMatching = false;
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            if (nav.remainingDistance <= nav.stoppingDistance)
            {
                _isMoving = false;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke(this);
            onDeadEvent = null;
        }

        protected override void Attack()
        {
            if (_isMoving) return;

            _anim.SetTrigger(IsAttack);
            StackObjectPool.Get("SlashVFX", _attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            audioSource.PlayOneShot(atkAudio);
            meleeWeapon.Attack(Target, Damage);
        }

        public void GoToTargetPosition(Vector3 pos)
        {
            _isMoving = true;
            _mousePos = pos;
            nav.SetDestination(_mousePos);
        }
    }
}