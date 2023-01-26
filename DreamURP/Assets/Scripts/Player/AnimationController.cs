using System;
using UnityEngine;

namespace Player
{
    public class AnimationController : MonoBehaviour
    {
        private Animator anim;
        private static readonly int IsMove = Animator.StringToHash("isMove");
        private static readonly int IsDeath = Animator.StringToHash("isDeath");
        private static readonly int IsHit = Animator.StringToHash("isHit");
        private static readonly int IsRoll = Animator.StringToHash("isRoll");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        
        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public void MoveAnimation(bool isMove)
        {
            anim.SetBool(IsMove,isMove);
        }

        public void RollAnimation()
        {
            anim.SetTrigger(IsRoll);
        }

        public void HitAnimation()
        {
            anim.SetTrigger(IsHit);
        }

        public void DeathAnimation()
        {
            anim.SetTrigger(IsDeath);
        }

        public void AttackAnimation()
        {
            anim.SetTrigger(IsAttack);
        }
    }
}