using GameControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class MeleeEnemy : EnemyUnit
    {
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Animator _anim;

        protected override void Awake()
        {
            base.Awake();
            _anim = GetComponent<Animator>();
        }

        protected override void Update()
        {
            
        }

        protected override void Attack()
        {
            _anim.SetTrigger(IsAttack);
            if (Target.TryGetComponent(out Health h))
            {
                h.TakeDamage(Damage, Target.gameObject);
            }
        }
    }
}