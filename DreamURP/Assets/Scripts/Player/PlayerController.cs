// using UnityEngine;
// using UnityEngine.InputSystem;
//
// namespace Player
// {
//     public class PlayerController : MonoBehaviour
//     {
//         //Component
//         private KnockBack knockBack;
//         private Rigidbody2D rigid;
//         private Animator anim;
//         private WeaponParent weaponParent;
//
//         [SerializeField] private ParticleSystem dust;
//         // public SpriteLibrary spriteLibrary;
//
//         //Input
//         [SerializeField] private InputActionReference movement, attack, roll;
//         private bool attackKey, rollKey;
//
//         [SerializeField] private bool isMove, isRoll, isHit;
//
//         //MoveControl
//         private Vector2 moveVec;
//         [SerializeField] private float moveSpeed;
//         [SerializeField] private float rollCoolTime;
//         [SerializeField] private float rollSpeed;
//
//         private static readonly int MoveAnim = Animator.StringToHash("isMove");
//         private static readonly int RollAnim = Animator.StringToHash("isRoll");
//         private static readonly int DeathAnim = Animator.StringToHash("isDeath");
//         private static readonly int HitAnim = Animator.StringToHash("isHit");
//
//         private void Awake()
//         {
//             knockBack = GetComponent<KnockBack>();
//             rigid = GetComponent<Rigidbody2D>();
//             anim = GetComponentInChildren<Animator>();
//             weaponParent = GetComponentInChildren<WeaponParent>();
//             // spriteLibrary = GetComponentInChildren<SpriteLibrary>();
//         }
//
//         private void Update()
//         {
//             if (isHit) return;
//             attackKey = attack.action.IsPressed();
//             rollKey = roll.action.IsPressed();
//             Move();
//             Roll();
//             Flip();
//             Attack();
//         }
//
//         private void CreateDust() => dust.Play();
//
//         private void Move()
//         {
//             if (isRoll) return;
//             moveVec = movement.action.ReadValue<Vector2>().normalized;
//             rigid.velocity = moveVec * moveSpeed;
//             isMove = moveVec.sqrMagnitude != 0;
//             anim.SetBool(MoveAnim, isMove);
//         }
//
//         private void Roll()
//         {
//             if (!rollKey || !isMove || isRoll || attackKey) return;
//             isRoll = true;
//             anim.SetTrigger(RollAnim);
//             rigid.velocity = moveVec * rollSpeed;
//             this.Wait(rollCoolTime, () => isRoll = false);
//         }
//
//         private void Flip()
//         {
//             if (attackKey || weaponParent.isAttack) return;
//             Vector2 scale = transform.localScale;
//             switch (moveVec.x)
//             {
//                 case < 0:
//                     scale.x = -1;
//                     CreateDust();
//                     break;
//                 case > 0:
//                     scale.x = 1;
//                     CreateDust();
//                     break;
//             }
//
//             transform.localScale = scale;
//         }
//
//         private void Attack()
//         {
//             if (attackKey && !isRoll)
//             {
//                 weaponParent.Attack();
//             }
//         }
//
//         public void HitAnimation()
//         {
//             if (isRoll) return;
//             isHit = true;
//             anim.SetTrigger(HitAnim);
//             // this.Wait(knockBack.knockbackTime, () => isHit = false);
//         }
//
//         public void DeathAnimation() => anim.SetTrigger(DeathAnim);
//     }
// }