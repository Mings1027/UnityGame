using System;
using DataControl;
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

        [SerializeField] private MeleeWeapon meleeWeapon;

        protected override void Awake()
        {
            base.Awake();
            _anim = GetComponent<Animator>();
            _attackEffectPos = transform.GetChild(0);
        }

        protected override void Attack()
        {
            if (isMoving) return;

            _anim.SetTrigger(IsAttack);
            ObjectPoolManager.Get(PoolObjectName.SwordSlashSfx, transform);
            ObjectPoolManager.Get(PoolObjectName.SlashVFX, _attackEffectPos.position,
                transform.rotation * Quaternion.Euler(0, 90, 0));
            meleeWeapon.Attack(Target, Damage);
        }
    }
}