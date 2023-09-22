using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using TowerControl;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour, IFingerUp
    {
        private AudioSource _audioSource;
        private Animator _anim;
        private Rigidbody _rigid;
        private SphereCollider _sphereCollider;
        private Health _health;
        private Transform _target;
        private Collider[] _targetCollider;
        private UnitTower _parentTower;
        private AttackPoint _attackPoint;

        private Vector3 _moveDir;
        private Vector3 _moveVec;

        private Vector3 _touchPos;
        private Vector3 _curPos;

        private TowerType _towerName;
        private int _damage;

        private Cooldown _atkCooldown;
        private Cooldown _targetingTime;

        private bool _startTargeting;
        private bool _isTargeting;
        private bool _moveInput;
        private bool _targetInAtkRange;
        private bool _isOnDestination;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        [SerializeField] private MeshRenderer indicator;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float atkRange;
        [SerializeField] private float sightRange;
        [SerializeField] private float moveSpeed;

        public event Action<FriendlyUnit> OnReSpawnEvent;

        public MeshRenderer Indicator => indicator;

        /*==============================================================================================================================================
                                                    Unity Event
==============================================================================================================================================*/

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _rigid = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
            _health = GetComponent<Health>();
            _targetCollider = new Collider[2];
            _attackPoint = GetComponentInChildren<AttackPoint>();
            _targetingTime.cooldownTime = 1f;
        }

        private void OnEnable()
        {
            _target = null;
            _isOnDestination = true;
            _curPos = transform.position;
            _health.OnDeadEvent += DeadAnimation;
            indicator.enabled = false;
        }

        private void FixedUpdate()
        {
            if (_health.IsDead) return;
            if (_moveInput) return;
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;

            if (!_startTargeting) return;
            Targeting();
            CheckRange();

            if (_isTargeting)
            {
                if (!_targetInAtkRange)
                    Move(_target.position);
            }
            else
            {
                if (!_isOnDestination)
                    Move(_curPos);
            }
        }

        private void Update()
        {
            if (!_startTargeting) return;
            if (_health.IsDead) return;
            if (_moveInput) return;

            Attack();
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _moveInput || (_isTargeting ? !_targetInAtkRange : !_isOnDestination));
        }

        private void OnDisable()
        {
            OnReSpawnEvent?.Invoke(this);
            OnReSpawnEvent = null;
            _health.OnDeadEvent -= DeadAnimation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        public void StartTargeting(bool startTargeting)
        {
            _startTargeting = startTargeting;
        }

        private void CheckRange()
        {
            if (_isTargeting)
            {
                _targetInAtkRange = Vector3.SqrMagnitude(_rigid.position - _target.position) < atkRange;
            }
            else
            {
                _isOnDestination = Vector3.SqrMagnitude(_rigid.position - _curPos) < 0.5f;
            }
        }

        private void Targeting()
        {
            if (_targetingTime.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                return;
            }

            _target = _targetCollider[0].transform;
            _isTargeting = true;
            _targetingTime.StartCooldown();
        }

        private void Attack()
        {
            if (!_isTargeting) return;
            if (!_targetInAtkRange) return;
            if (_atkCooldown.IsCoolingDown) return;
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            DataManager.SumDamage(_towerName, _damage);
            _atkCooldown.StartCooldown();
            if (_target.gameObject.activeSelf) return;
            _target = null;
            _isTargeting = false;
        }

        private void Move(Vector3 targetPos)
        {
            var pos = _rigid.position;
            _moveDir = (targetPos - pos).normalized;
            _moveVec = _moveDir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(pos + _moveVec);
            _rigid.MoveRotation(Quaternion.LookRotation(_moveDir));
        }

        private void DeadAnimation()
        {
            var pos = transform.position;
            _rigid.DORotate(new Vector3(-90, pos.y, pos.z), 0.5f, RotateMode.LocalAxisAdd).OnComplete(() =>
            {
                _rigid.transform.DOScale(0, 1).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _rigid.transform.localScale = Vector3.one;
                });
            });
        }

        public async UniTask MoveToTouchPos(Vector3 pos)
        {
            _curPos = pos;
            _moveInput = true;
            _sphereCollider.enabled = false;
            _rigid.MoveRotation(Quaternion.LookRotation(pos - _rigid.position));
            await _rigid.DOMove(pos, moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() =>
            {
                _moveInput = false;
                _sphereCollider.enabled = true;
            }).ToUniTask();
        }

        public void Init(UnitTower unitTower, TowerType towerName, int damage, float attackDelay, float healthAmount)
        {
            _parentTower = unitTower;
            _atkCooldown.cooldownTime = attackDelay;
            _towerName = towerName;
            _damage = damage;
            _health.Init(healthAmount);
        }

        public void FingerUp()
        {
            _parentTower.FingerUp();
        }
    }
}