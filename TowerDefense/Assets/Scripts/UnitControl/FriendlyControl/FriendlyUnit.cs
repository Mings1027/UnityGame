using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using StatusControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl.FriendlyControl
{
    public class FriendlyUnit : MonoBehaviour
    {
        private Animator _anim;
        private Rigidbody _rigid;
        private Health health;
        private Collider[] _targetColliders;
        private CancellationTokenSource cts;
        private AttackPoint attackPoint;
        private Transform t;

        private Vector3 touchPos;

        private float _atkDelay;
        private bool _isTargeting;
        private bool _isMoving;
        private bool isAttack;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private Transform target;

        public event Action<FriendlyUnit> OnDeadEvent;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] private float moveSpeed;

/*==============================================================================================================================================
                                                    Unity Event
==============================================================================================================================================*/

        private void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _rigid = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
            _targetColliders = new Collider[2];
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
            _isMoving = false;
            isAttack = false;
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void FixedUpdate()
        {
            if (_isMoving) return;
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
            _anim.SetBool(IsWalk, _isMoving || _isTargeting);
        }

        private void OnDisable()
        {
            cts?.Cancel();
            CancelInvoke();
            OnDeadEvent?.Invoke(this);
            OnDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(t.position, atkRange);
        }

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void ChaseTarget()
        {
            var targetPos = target.position;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
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
            if (_isMoving) return;
            target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            _isTargeting = target != null;
        }

        public void MoveToTouchPos(Vector3 pos)
        {
            _isMoving = true;
            _rigid.DOMove(pos, moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => _isMoving = false);
            _rigid.MoveRotation(Quaternion.LookRotation(pos - _rigid.position));
        }

        public void Init(int damage, float attackDelay, float healthAmount)
        {
            attackPoint.damage = damage;
            _atkDelay = attackDelay;
            health.Init(healthAmount);
        }
    }
}