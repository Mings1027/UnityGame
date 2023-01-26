// using System.Collections.Generic;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace Enemy
// {
//     public class EnemyController : MonoBehaviour
//     {
//         [SerializeField] private Transform target;
//         [SerializeField] private LayerMask checkLayer;
//         // [SerializeField] private Transform attackParent;
//
//         //Component
//         private Animator anim;
//         private SpriteRenderer spriteRenderer;
//         private Rigidbody2D rigid;
//
//         [SerializeField] private float moveSpeed;
//
//         //RandomMove
//         private readonly List<Vector2> ranMoveList = new();
//         private Vector3 dir;
//         private float moveTime;
//         [SerializeField] private float minMoveTime, maxMoveTime;
//         private float waitTime;
//         [SerializeField] private float minWaitTime, maxWaitTime;
//
//         //Attack
//         [SerializeField] private float chaseRange;
//         private float delay;
//         [SerializeField] private float atkCoolTime;
//
//         //Bool
//         [SerializeField] private bool isMove, isChase, isAttack, isHit;
//
//         //Animation        
//         private static readonly int MoveAnim = Animator.StringToHash("isMove");
//         private static readonly int HitAnim = Animator.StringToHash("isHit");
//         private static readonly int DeathAnim = Animator.StringToHash("isDeath");
//         private static readonly int IsAttack = Animator.StringToHash("isAttack");
//
//
//         private void OnEnable()
//         {
//             anim = GetComponentInChildren<Animator>();
//             spriteRenderer = GetComponentInChildren<SpriteRenderer>();
//             rigid = GetComponent<Rigidbody2D>();
//             GetComponent<KnockBack>();
//             ranMoveList.Add(Vector2.right);
//             ranMoveList.Add(Vector2.left);
//             ranMoveList.Add(Vector2.up);
//             ranMoveList.Add(Vector2.down);
//             ranMoveList.Add(new Vector2(1, 1));
//             ranMoveList.Add(new Vector2(-1, 1));
//             ranMoveList.Add(new Vector2(1, -1));
//             ranMoveList.Add(new Vector2(-1, -1));
//             dir = ranMoveList[Random.Range(0, 8)];
//             waitTime = Random.Range(minWaitTime, maxWaitTime);
//             moveTime = Random.Range(minMoveTime, maxMoveTime);
//             delay = atkCoolTime;
//         }
//
//         private void OnDisable()
//         {
//             ObjectPooler.ReturnToPool(gameObject);
//         }
//
//         private void Update()
//         {
//             anim.SetBool(MoveAnim, isMove);
//             if (isHit || isAttack) return;
//             var position = transform.position;
//             var dis = target.position - position;
//             var hit = Physics2D.Raycast(position, dis, chaseRange, checkLayer);
//             if (dis.magnitude < chaseRange && hit.collider.CompareTag("Player"))
//             {
//                 Debug.DrawRay(transform.position, dis, Color.green);
//                 Chase();
//             }
//             else
//             {
//                 RandomMove();
//             }
//
//             Flip();
//         }
//
//         private void Chase()
//         {
//             isChase = true;
//             isMove = true;
//             Vector2 dis = target.position - transform.position;
//             rigid.velocity = dis.normalized * moveSpeed;
//         }
//
//         private void RandomMove()
//         {
//             isAttack = false;
//             isChase = false;
//             if (moveTime <= 0)
//             {
//                 if (waitTime <= 0)
//                 {
//                     dir = ranMoveList[Random.Range(0, 8)];
//                     waitTime = Random.Range(minWaitTime, maxWaitTime);
//                     moveTime = Random.Range(minMoveTime, maxMoveTime);
//                 }
//                 else
//                 {
//                     isMove = false;
//                     waitTime -= Time.deltaTime;
//                     rigid.velocity = Vector2.zero;
//                 }
//             }
//             else
//             {
//                 isMove = true;
//                 moveTime -= Time.deltaTime;
//                 rigid.velocity = dir.normalized * moveSpeed;
//             }
//         }
//
//         private void Flip()
//         {
//             if (isAttack) return;
//             var x = spriteRenderer.flipX;
//             if (isChase)
//             {
//                 if (target.position.x > transform.position.x) x = false;
//                 else if (target.position.x < transform.position.x) x = true;
//             }
//             else
//             {
//                 if (dir.x > 0) x = false;
//                 else if (dir.x < 0) x = true;
//             }
//
//             spriteRenderer.flipX = x;
//         }
//
//         public void HitAnimation()
//         {
//             isHit = true;
//             anim.SetTrigger(HitAnim);
//             //this.Wait(knockBack.knockbackTime, () => isHit = false);
//         }
//
//         public void DeathAnimation()
//         {
//             anim.SetTrigger(DeathAnim);
//         }
//
//         private void OnTriggerEnter2D(Collider2D col)
//         {
//             if (!col.CompareTag("Wall")) return;
//             dir = ranMoveList[Random.Range(0, 8)];
//         }
//
//         private void OnTriggerStay2D(Collider2D col)
//         {
//             if (!col.CompareTag("Player")) return;
//             rigid.velocity = Vector2.zero;
//             rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;
//             isMove = false;
//             isAttack = true;
//             if (delay <= 0) //Attack
//             {
//                 anim.SetTrigger(IsAttack);
//                 var health = col.GetComponent<Health>();
//                 if (health != null)
//                     health.GetHit(1, transform.gameObject);
//                 delay = atkCoolTime;
//             }
//             else
//             {
//                 delay -= Time.deltaTime;
//             }
//         }
//
//         private void OnTriggerExit2D(Collider2D other)
//         {
//             if (!other.CompareTag("Player")) return;
//             rigid.sleepMode = RigidbodySleepMode2D.StartAwake;
//             isMove = true;
//             isAttack = false;
//         }
//
//         private void OnDrawGizmosSelected()
//         {
//             var position = transform.position;
//             Gizmos.DrawWireSphere(position, chaseRange);
//         }
//     }
// }