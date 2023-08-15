using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
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
        private UnitHealth unitHealth;
        private Collider[] _targetColliders;
        private CancellationTokenSource cts;

        private Vector3 touchPos;

        private int _damage;
        private float _atkDelay;
        private bool _isMoving;
        private bool _isTargeting;
        private bool _isChasing;
        private bool isCoolingDown;

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
            unitHealth = GetComponent<UnitHealth>();
            _rigid = GetComponent<Rigidbody>();
            _targetColliders = new Collider[2];
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            isCoolingDown = true;

            target = null;
            _isTargeting = false;
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void Update()
        {
            if (_isMoving) return;
            if (!_isTargeting) return;

            if (Vector3.Distance(transform.position, target.position) > 1)
            {
                _isChasing = true;
                ChaseTarget();
            }
            else
            {
                _isChasing = false;
                DoAttack();
            }
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _isMoving || _isChasing);
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
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        protected virtual void Attack()
        {
            if (target.TryGetComponent(out EnemyHealth h))
            {
                h.TakeDamage(_damage);
            }
        }

        private async UniTaskVoid StartCoolDown()
        {
            isCoolingDown = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            isCoolingDown = true;
        }

        private void ChaseTarget()
        {
            var targetPos = target.position + new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * 2;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
            _rigid.MoveRotation(Quaternion.LookRotation(target.position - _rigid.position, Vector3.up));
        }

        private void DoAttack()
        {
            if (!isCoolingDown) return;

            if (!target.gameObject.activeSelf)
            {
                target = null;
                _isTargeting = false;
                return;
            }

            _anim.SetTrigger(IsAttack);
            Attack();
            StartCoolDown().Forget();
        }

        private void Targeting()
        {
            if (_isMoving) return;
            target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            _isTargeting = target != null;
        }

        public void MoveToTouchPos(Vector3 pos)
        {
            _isMoving = true;
            pos = new Vector3(pos.x + Random.insideUnitCircle.x, pos.y, pos.z + Random.insideUnitCircle.y);
            _rigid.DOMove(pos, moveSpeed).SetSpeedBased().OnComplete(() => _isMoving = false);
            _rigid.MoveRotation(Quaternion.LookRotation(pos - _rigid.position));
        }

        public void Init(int damage, float attackDelay, float health)
        {
            _damage = damage;
            _atkDelay = attackDelay;
            unitHealth.Init(health);
        }
    }
}