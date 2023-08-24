using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using StatusControl;
using TowerControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Animator _anim;
        private Rigidbody rigid;
        private EnemyAI _enemyAI;
        private Collider[] targetCollider;
        private CancellationTokenSource cts;
        private AttackPoint attackPoint;
        private Transform target;
        private Transform t;

        private bool isAttack;
        private bool _isTargeting;
        private int _attackRange;
        private float _atkDelay;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        [SerializeField] private LayerMask targetLayer;
/*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            rigid = GetComponent<Rigidbody>();
            _enemyAI = GetComponent<EnemyAI>();
            targetCollider = new Collider[3];
            attackPoint = GetComponentInChildren<AttackPoint>();
            t = transform;
            attackPoint.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            target = null;
            _isTargeting = false;
            isAttack = false;
            InvokeRepeating(nameof(Targeting), 0, 1);
        }

        private void FixedUpdate()
        {
            if (!_isTargeting) return;

            if (Vector3.Distance(t.position, target.position) > 1)
            {
                ChaseTarget();
            }
            else
            {
                DoAttack();
            }
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _enemyAI.CanMove);
        }

        private void OnDisable()
        {
            cts?.Cancel();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(t.position, _attackRange);
        }
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void ChaseTarget()
        {
            var targetPos = target.position;
            var position = rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (_enemyAI.MoveSpeed * Time.deltaTime);
            rigid.MovePosition(position + moveVec);
            t.forward = (targetPos - position).normalized;
        }

        private void DoAttack()
        {
            if (isAttack) return;
            if (!target.gameObject.activeSelf)
            {
                target = null;
                _isTargeting = false;
                return;
            }

            t.forward = (target.position - t.position).normalized;
            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            _isTargeting = false;
            isAttack = true;
            _anim.SetTrigger(IsAttack);
            attackPoint.target = target;
            attackPoint.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            attackPoint.gameObject.SetActive(false);
            isAttack = false;
        }

        private void Targeting()
        {
            if (isAttack) return;
            target = SearchTarget.ClosestTarget(transform.position, _attackRange, targetCollider, targetLayer);
            _isTargeting = target != null;
            _enemyAI.CanMove = !_isTargeting;
        }

        public void Init(Wave wave)
        {
            _attackRange = wave.atkRange;
            _atkDelay = wave.atkDelay;
            attackPoint.damage = wave.damage;
        }
    }
}