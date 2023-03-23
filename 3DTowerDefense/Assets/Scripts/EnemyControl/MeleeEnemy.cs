using GameControl;
using UnityEngine;

namespace EnemyControl
{
    public class MeleeEnemy : Enemy
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;

        protected override void Awake()
        {
            base.Awake();
            _anim = GetComponent<Animator>();
        }

        protected override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            if (targetFinder.Target.TryGetComponent(out Health h))
            {
                h.GetHit(targetFinder.Damage, targetFinder.Target.gameObject);
            }
        }
    }
}