using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : MonoBehaviour
    {
        private Animator anim;
        private EnemyAI _enemyAI;
        private Health _health;
        private Collider[] targetCollider;
        private CancellationTokenSource cts;

        private bool isCoolingDown;
        private bool _isSpeedDeBuffed;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        protected Transform Target { get; private set; }
        protected int damage;

        private bool isTargeting;
        private int _attackRange;
        private float _atkDelay;

        [SerializeField] private LayerMask targetLayer;
/*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            _enemyAI = GetComponent<EnemyAI>();
            _health = GetComponent<Health>();
            targetCollider = new Collider[3];
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            isCoolingDown = true;
            Target = null;
            isTargeting = false;
            InvokeRepeating(nameof(Targeting), 0, 1);
        }

        private void Update()
        {
            if (isTargeting)
            {
                DoAttack();
            }
        }

        private void LateUpdate()
        {
            anim.SetBool(IsWalk, _enemyAI.CanMove);
        }

        private void OnDisable()
        {
            cts?.Cancel();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DefenseTower"))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void DoAttack()
        {
            if (!isCoolingDown) return;
            if (!Target.gameObject.activeSelf)
            {
                Target = null;
                isTargeting = false;
                _enemyAI.CanMove = true;
                return;
            }

            anim.SetTrigger(IsAttack);
            Attack();
            StartCoolDown().Forget();
        }

        protected virtual void Attack()
        {
            if (Target.TryGetComponent(out Health h))
            {
                h.TakeDamage(damage);
            }
        }

        private async UniTaskVoid StartCoolDown()
        {
            isCoolingDown = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            isCoolingDown = true;
        }

        public async UniTaskVoid SlowMovement(float deBuffTime, float decreaseSpeed)
        {
            if (_isSpeedDeBuffed) return;
            _isSpeedDeBuffed = true;
            _enemyAI.MoveSpeed -= decreaseSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(deBuffTime), cancellationToken: cts.Token);
            _enemyAI.MoveSpeed += decreaseSpeed;
            _isSpeedDeBuffed = false;
        }

        private void Targeting()
        {
            Target = SearchTarget.ClosestTarget(transform.position, _attackRange, targetCollider, targetLayer);
            isTargeting = Target != null;
            _enemyAI.CanMove = !isTargeting;
        }

        public void Init(int attackRange, float attackDelay, int attackDamage, float health)
        {
            _attackRange = attackRange;
            _atkDelay = attackDelay;
            damage = attackDamage;
            _health.Init(health);
        }
    }
}