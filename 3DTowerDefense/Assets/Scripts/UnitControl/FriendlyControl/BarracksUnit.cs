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

        [SerializeField] private MeleeWeapon meleeWeapon;
        [SerializeField] private AudioClip atkAudio;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            _anim = GetComponent<Animator>();
            _attackEffectPos = transform.GetChild(0);
        }

        protected override void Attack()
        {
            if (isMoving) return;

            _anim.SetTrigger(IsAttack);
            StackObjectPool.Get("SlashVFX", _attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            audioSource.PlayOneShot(atkAudio);
            meleeWeapon.Attack(Target, Damage);
        }
    }
}