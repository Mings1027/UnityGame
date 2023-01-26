using UnityEngine;

namespace Player
{
    public class PlayerMover : MonoBehaviour
    {
        private Rigidbody2D rigid;
        private WeaponParent weaponParent;
        private AnimationController animationController;
        private Status status;
        private ParticleSystem dust;

        private Vector2 moveVec;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float rollSpeed, rollCoolTime;
        [SerializeField] private float atkDelay;
        [SerializeField] private bool isMove, isRoll, isAttack;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            weaponParent = GetComponentInChildren<WeaponParent>();
            animationController = GetComponentInChildren<AnimationController>();
            status = GetComponent<Status>();
            dust = GetComponentInChildren<ParticleSystem>();
        }

        public void Movement(Vector2 movement)
        {
            if (isRoll) return;
            isMove = moveVec.sqrMagnitude > 0;
            moveVec = movement;
            animationController.MoveAnimation(isMove);
            rigid.velocity = moveVec * moveSpeed;
            CreateDust();
        }

        public void Roll()
        {
            if (isRoll || !isMove || isAttack || !status.RollStamina()) return;
            isRoll = true;
            animationController.RollAnimation();
            rigid.velocity = moveVec * rollSpeed;
            this.Wait(rollCoolTime, () => isRoll = false);
        }

        public void Attack()
        {
            if (isRoll || isAttack || !status.AttackStamina()) return;
            isAttack = true;
            weaponParent.Attack();
            this.Wait(atkDelay, () => isAttack = false);
        }


        public void Flip()
        {
            if (isRoll || isAttack) return;
            var thisTransform = transform;
            Vector2 scale = thisTransform.localScale;
            scale.x = moveVec.x switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => scale.x
            };
            thisTransform.localScale = scale;
        }

        public void Dead()
        {
            rigid.velocity = Vector2.zero;
        }

        private void CreateDust()
        {
            if (moveVec.sqrMagnitude == 0) return;
            dust.Play();
        }
    }
}