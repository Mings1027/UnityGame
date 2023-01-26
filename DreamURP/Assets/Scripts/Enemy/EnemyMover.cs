using System.Collections.Generic;
using Pathfinding;
using Player;
using UnityEngine;
using static UnityEngine.Physics2D;
using static UnityEngine.Random;
using static UnityEngine.Time;
using static UnityEngine.Vector2;

namespace Enemy
{
    public class EnemyMover : MonoBehaviour
    {
        private Rigidbody2D rigid;
        private AnimationController animationController;
        private SpriteRenderer spriteRenderer;
        [SerializeField] private int moveSpeed;

        private readonly List<Vector2> ranMoveList = new();
        private Vector3 dir;
        private float moveTime;
        private float waitTime;
        [SerializeField] private float maxTime;
        private float atkDelay;
        [SerializeField] private float atkCoolTime;
        [SerializeField] private bool isAttack, isChase;

        private AIPath aiPath;

        private void OnEnable()
        {
            rigid = GetComponentInChildren<Rigidbody2D>();
            animationController = GetComponentInChildren<AnimationController>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            aiPath = GetComponent<AIPath>();
            ranMoveList.Add(left + up);
            ranMoveList.Add(left);
            ranMoveList.Add(left + down);
            ranMoveList.Add(up);
            ranMoveList.Add(down);
            ranMoveList.Add(right + up);
            ranMoveList.Add(right);
            ranMoveList.Add(right + down);
            dir = ranMoveList[Range(0, 8)];
            waitTime = Range(1, maxTime);
            moveTime = Range(1, maxTime);
        }

        public void StartChase()
        {
            isChase = true;
            animationController.MoveAnimation(true);
        }

        public void StopChase()
        {
            isChase = false;
            animationController.MoveAnimation(false);
        }

        public void Attack()
        {
            isChase = false;
            animationController.MoveAnimation(false);
            if (atkDelay <= 0)
            {
                atkDelay = atkCoolTime;
                isAttack = true;
                animationController.AttackAnimation();
                this.Wait(atkCoolTime, () => isAttack = false);
            }
            else atkDelay -= deltaTime;
        }

        public void RandomMove(float attackRange)
        {
            isChase = false;
            animationController.MoveAnimation(rigid.velocity != zero);
            if (moveTime <= 0)
            {
                if (waitTime <= 0)
                {
                    //change direction
                    dir = ranMoveList[Range(0, 8)];
                    waitTime = Range(1, maxTime);
                    moveTime = Range(1, maxTime);
                }
                else
                {
                    //stay
                    waitTime -= deltaTime;
                    rigid.velocity = zero;
                }
            }
            else
            {
                //move
                moveTime -= deltaTime;
                rigid.velocity = dir.normalized * moveSpeed;
                if (Raycast(transform.position, dir.normalized, attackRange, LayerMask.GetMask("Wall")))
                    dir = ranMoveList[Range(0, 8)];
            }
        }

        public void Flip(Vector3 player)
        {
            if (isAttack) return;
            if (aiPath.desiredVelocity.x > 0.01f || rigid.velocity.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (aiPath.desiredVelocity.x < -0.01f || rigid.velocity.x < 0)
            {
                Debug.Log("fdjios");
                spriteRenderer.flipX = true;
            }
            // var flipX = isChase ? player.x < transform.position.x : dir.x < 0;
            // spriteRenderer.flipX = flipX;
        }

        public void Dead()
        {
            rigid.velocity = zero;
        }
    }
}