using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using Pathfinding;
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
        private UnitAI _unitAI;
        private Health _health;
        private Collider[] _targetCollider;
        private UnitTower _parentTower;
        private AttackPoint _attackPoint;
        private Cooldown atkCooldown;
        private Collider _target;

        private Vector3 _moveDir;
        private Vector3 _moveVec;

        private TowerType _towerType;
        private int _damage;
        private byte curWayPoint;

        private bool _isTargeting;
        private bool _moveInput;
        private bool _targetInAtkRange;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        private static readonly int IsDead = Animator.StringToHash("isDead");

        [SerializeField] private MeshRenderer indicator;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float atkRange;
        [SerializeField] private float sightRange;
        [SerializeField] private float moveSpeed;

        public event Action OnReSpawnEvent;

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
            _unitAI = GetComponent<UnitAI>();
            _health = GetComponent<Health>();
            _targetCollider = new Collider[2];
            _attackPoint = GetComponentInChildren<AttackPoint>();
        }

        private void OnEnable()
        {
            _target = null;
            _targetInAtkRange = false;
            _health.OnDeadEvent += DeadAnimation;
            indicator.enabled = false;
        }

        private void OnDisable()
        {
            if (_health.IsDead)
            {
                OnReSpawnEvent?.Invoke();
                OnReSpawnEvent = null;
            }

            _isTargeting = false;
            _moveInput = false;
            _targetInAtkRange = false;
            // _parentTower = null;
            // _towerType = TowerType.None;
            CancelInvoke();
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            if (_target == null) return;
            Gizmos.DrawSphere(_target.transform.position, 0.5f);
        }
#endif
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        public void UnitFixedUpdate()
        {
            if (_health.IsDead) return;
            if (_moveInput) return;
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }

        public void UnitUpdate()
        {
            if (_health.IsDead) return;
            _anim.SetBool(IsWalk, _moveInput || _unitAI.CanMove);

            if (_targetInAtkRange && _isTargeting)
            {
                _unitAI.CanMove = false;
                if (atkCooldown.IsCoolingDown) return;
                Attack();
                atkCooldown.StartCooldown();
            }
        }

        public void TargetInit()
        {
            _anim.SetBool(IsWalk, false);
            _isTargeting = false;
            _target = null;
        }

        public void UnitTargeting()
        {
            if (_moveInput) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _unitAI.CanMove = false;
                return;
            }

            if (_target)
            {
                if (!_target.enabled)
                {
                    _target = null;
                    _isTargeting = false;
                }
            }

            if (!_isTargeting)
            {
                _isTargeting = true;
                _target = _targetCollider[0];
                _unitAI.CanMove = true;
            }

            if (!_target) return;
            var targetPos = _target.transform.position;
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= atkRange;
            _unitAI.targetPos = targetPos;
        }

        private void Attack()
        {
            if (_moveInput) return;
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            DataManager.SumDamage(_towerType, _damage);
        }

        private void DeadAnimation()
        {
            _anim.SetTrigger(IsDead);
            DOVirtual.DelayedCall(2, () => gameObject.SetActive(false));
        }

        public async UniTask MoveToTouchPosTest(Vector3 pos)
        {
            _moveInput = true;
            _sphereCollider.enabled = false;
            _anim.SetBool(IsWalk, true);
            _unitAI.CanMove = true;
            _unitAI.targetPos = pos;
            _unitAI.enabled = false;
            _unitAI.enabled = true;

            await UniTask.WaitUntil(_unitAI.reachedEndOfPath);

            _moveInput = false;
            _sphereCollider.enabled = true;
            _anim.SetBool(IsWalk, false);
            _unitAI.CanMove = false;
            if (TowerManager.Instance.StartWave) return;
            _unitAI.enabled = false;
        }

        public void Init(UnitTower unitTower, TowerType towerType)
        {
            _parentTower = unitTower;
            _towerType = towerType;
        }

        public void UnitUpgrade(int damage, float healthAmount, float delay)
        {
            _damage = damage;
            _health.Init(healthAmount);
            _unitAI.targetPos = transform.position;
            atkCooldown.cooldownTime = delay;
        }

        public void FingerUp()
        {
            _parentTower.FingerUp();
        }
    }
}