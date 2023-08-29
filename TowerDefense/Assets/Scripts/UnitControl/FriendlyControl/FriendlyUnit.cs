using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour
    {
        private Animator _anim;
        private Rigidbody _rigid;
        private Health _health;
        private Collider[] _targetColliders;
        private CancellationTokenSource _cts;
        private Transform _t;
        private Transform _target;

        private Vector3 _touchPos;

        private int _damage;
        private float _atkDelay;
        private bool _isTargeting;
        private bool _isMoving;
        private bool _isAttack;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public event Action<FriendlyUnit> OnDeadEvent;
        public string towerType { get; set; }

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
            _health = GetComponent<Health>();
            _targetColliders = new Collider[2];
            _t = transform;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _target = null;
            _isTargeting = false;
            _isMoving = false;
            _isAttack = false;
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void FixedUpdate()
        {
            if (_isMoving) return;
            if (!_isTargeting) return;

            if (Vector3.Distance(_t.position, _target.position) > 1)
            {
                ChaseTarget();
            }
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _isMoving || _isTargeting);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
            OnDeadEvent?.Invoke(this);
            OnDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_t.position, atkRange);
        }

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void ChaseTarget()
        {
            var targetPos = _target.position;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
            _t.forward = (targetPos - position).normalized;
        }

        private void DoAttack()
        {
            if (_isAttack) return;
            if (!_target.gameObject.activeSelf)
            {
                _target = null;
                _isTargeting = false;
                return;
            }

            _t.forward = (_target.position - _t.position).normalized;
            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            _isTargeting = false;
            _isAttack = true;
            _anim.SetTrigger(IsAttack);
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
                DataManager.Instance.SumDamage(towerType, _damage);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            _isAttack = false;
        }

        private void Targeting()
        {
            if (_isAttack) return;
            if (_isMoving) return;
            _target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            _isTargeting = _target != null;

            if (_isTargeting)
            {
                if (Vector3.Distance(_t.position, _target.position) <= 1)
                {
                    DoAttack();
                }
            }
        }

        public void MoveToTouchPos(Vector3 pos)
        {
            _isMoving = true;
            _rigid.DOMove(pos, moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => _isMoving = false);
            _rigid.MoveRotation(Quaternion.LookRotation(pos - _rigid.position));
        }

        public void Init(int damage, float attackDelay, float healthAmount)
        {
            _damage = damage;
            _atkDelay = attackDelay;
            _health.Init(healthAmount);
        }
    }
}