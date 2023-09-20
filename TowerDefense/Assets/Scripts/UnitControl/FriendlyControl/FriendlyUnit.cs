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

        private Vector3 _touchPos;
        private Vector3 _curPos;

        private TowerType _towerName;
        private int _damage;

        private Cooldown _atkCooldown;
        private Cooldown _targetingTime;

        private bool _isTargeting;
        private bool _moveInput;
        private bool _targetInAtkRange;

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
            _targetingTime.cooldownTime = 2f;
        }

        private void OnEnable()
        {
            _target = null;
            _curPos = transform.position;
            _health.OnDeadEvent += DeadAnimation;
            indicator.enabled = false;
        }

        private void FixedUpdate()
        {
            if (_health.IsDead) return;
            if (_moveInput) return;
            _targetInAtkRange = Physics.CheckSphere(transform.position, atkRange, targetLayer);

            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }

        private void Update()
        {
            if (_health.IsDead) return;
            if (_moveInput) return;

            Targeting();
            if (_isTargeting)
            {
                if (_targetInAtkRange)
                {
                    Attack();
                }
                else
                {
                    Chase();
                }
            }
            else
            {
                ReturnToOriginalPos();
            }
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

        private void Targeting()
        {
            if (_targetingTime.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _anim.SetBool(IsWalk, false);
                return;
            }

            _target = _targetCollider[0].transform;
            _isTargeting = true;
            _targetingTime.StartCooldown();
        }

        private void Chase()
        {
            _anim.SetBool(IsWalk, true);
            var targetPos = _target.position;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
            transform.forward = (targetPos - position).normalized;
        }

        private void Attack()
        {
            if (_atkCooldown.IsCoolingDown) return;
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            DataManager.SumDamage(ref _towerName, _damage);
            _atkCooldown.StartCooldown();
            if (_target.gameObject.activeSelf) return;
            _target = null;
            _isTargeting = false;
        }

        private void ReturnToOriginalPos()
        {
            if (Vector3.SqrMagnitude(transform.position - _curPos) < 0.5f) return;
            _anim.SetBool(IsWalk, true);
            var dir = (_curPos - transform.position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            _rigid.MovePosition(_rigid.position + moveVec);
            _rigid.MoveRotation(Quaternion.LookRotation(_curPos - _rigid.position));
        }

        private void DeadAnimation()
        {
            var pos = transform.position;
            transform.DORotate(new Vector3(-90, pos.y, pos.z), 0.5f, RotateMode.LocalAxisAdd).OnComplete(() =>
            {
                transform.DOScale(0, 1).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                });
            });
        }

        public async UniTask MoveToTouchPos(Vector3 pos)
        {
            _anim.SetBool(IsWalk, true);
            _curPos = pos;
            _moveInput = true;
            _sphereCollider.enabled = false;
            _rigid.MoveRotation(Quaternion.LookRotation(pos - _rigid.position));
            await _rigid.DOMove(pos, moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() =>
            {
                _moveInput = false;
                _sphereCollider.enabled = true;
                _anim.SetBool(IsWalk, false);
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