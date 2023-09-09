using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private AudioSource _audioSource;
        private Animator _anim;
        private Rigidbody _rigid;
        private Health _health;
        private Collider[] _targetColliders;
        private CancellationTokenSource _cts;
        private Transform _t;
        private Transform _target;

        private Vector3 _touchPos;
        private Vector3 _curPos;

        private int _damage;
        private float _atkDelay;
        private bool _isTargeting;

        private bool _moveInput;
        private bool _isMoving;
        private bool _isAttack;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public event Action<FriendlyUnit> OnDeadEvent;
        public string towerType { get; set; }

        public UnitTower parentTower { get; set; }

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] private float moveSpeed;
        private static readonly int IsDead = Animator.StringToHash("isDead");

        /*==============================================================================================================================================
                                                    Unity Event
==============================================================================================================================================*/

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
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
            _moveInput = false;
            _isMoving = false;
            _isAttack = false;
            _curPos = transform.position;
            _health.OnDeadEvent += DeadAnimation;
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void FixedUpdate()
        {
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
            if (_moveInput) return;

            ReturnToOriginalPos();

            if (!_isTargeting) return;

            if (Vector3.Distance(_t.position, _target.position) > 1)
            {
                ChaseTarget();
            }
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _moveInput || _isMoving || _isTargeting);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
            OnDeadEvent?.Invoke(this);
            OnDeadEvent = null;
            _health.OnDeadEvent -= DeadAnimation;
            ObjectPoolManager.ReturnToPool(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (_t == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_t.position, atkRange);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            parentTower.OnPointerUp(eventData);
        }
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void ReturnToOriginalPos()
        {
            _isMoving = Vector3.SqrMagnitude(transform.position - _curPos) > 0.5f;

            if (_isTargeting || !_isMoving) return;

            var dir = (_curPos - transform.position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(_rigid.position + moveVec);
            _rigid.MoveRotation(Quaternion.LookRotation(_curPos - _rigid.position));
        }

        private void ChaseTarget()
        {
            _isMoving = true;
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
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        private async UniTaskVoid Attack()
        {
            _isTargeting = false;
            _isAttack = true;
            _isMoving = false;
            _anim.SetTrigger(IsAttack);
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                ObjectPoolManager.Get(StringManager.BloodVfx, _target.position);
                damageable.Damage(_damage);
                DataManager.SumDamage(towerType, _damage);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            _isAttack = false;
        }

        private void DeadAnimation() => _anim.SetTrigger(IsDead);

        private void Targeting()
        {
            if (_isAttack) return;
            if (_moveInput) return;
            _target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            _isTargeting = _target != null;

            if (_isTargeting)
            {
                if (Vector3.Distance(_t.position, _target.position) <= 1)
                {
                    _curPos = transform.position;
                    DoAttack();
                }
            }
        }

        public void MoveToTouchPos(Vector3 pos)
        {
            _curPos = pos;
            _moveInput = true;
            _rigid.DOMove(pos, moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => _moveInput = false);
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