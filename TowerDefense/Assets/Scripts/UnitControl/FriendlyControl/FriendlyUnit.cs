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
using UnityEngine.AI;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour, IFingerUp
    {
        private AudioSource _audioSource;
        private Animator _anim;
        private SphereCollider _sphereCollider;
        private UnitNavAI _unitNavAI;
        private NavMeshAgent _navMeshAgent;

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
        private float atkRange;
        [SerializeField] private float sightRange;

        public MeshRenderer Indicator => indicator;

        /*==============================================================================================================================================
                                                    Unity Event
==============================================================================================================================================*/

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _sphereCollider = GetComponent<SphereCollider>();
            _unitNavAI = GetComponent<UnitNavAI>();
            _unitNavAI.enabled = false;
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _targetCollider = new Collider[2];
            _attackPoint = GetComponentInChildren<AttackPoint>();
            atkRange = _sphereCollider.radius * 2f;
            _navMeshAgent.stoppingDistance = atkRange;
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
            _isTargeting = false;
            _moveInput = false;
            _targetInAtkRange = false;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
#endif
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        public void UnitUpdate()
        {
            if (_health.IsDead) return;
            _anim.SetBool(IsWalk, _moveInput || !_navMeshAgent.isStopped);

            if (!_targetInAtkRange || !_isTargeting) return;
            _navMeshAgent.isStopped = true;
            if (atkCooldown.IsCoolingDown) return;
            Attack();
            atkCooldown.StartCooldown();
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
            if (!_navMeshAgent.isActiveAndEnabled) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _navMeshAgent.isStopped = true;
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
                _navMeshAgent.isStopped = false;
            }

            if (!_target) return;
            var targetPos = _target.transform.position;
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= atkRange;
            _navMeshAgent.SetDestination(targetPos);
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
            DOVirtual.DelayedCall(2, () => gameObject.SetActive(false), false);
        }

        public async UniTask MoveToTouchPosTest(Vector3 pos)
        {
            _moveInput = true;
            _anim.SetBool(IsWalk, true);
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(pos);
            _unitNavAI.enabled = true;
            await UniTask.WaitUntil(_unitNavAI.isStopped);

            _moveInput = false;
            _anim.SetBool(IsWalk, false);
            if (TowerManager.Instance.StartWave) return;
            _unitNavAI.enabled = false;
        }

        public void Init(UnitTower unitTower, TowerType towerType)
        {
            _parentTower = unitTower;
            _towerType = towerType;
        }

        public void UnitUpgrade(int damage, int healthAmount, float delay)
        {
            _damage = damage;
            _health.Init(healthAmount);
            atkCooldown.cooldownTime = delay;
        }

        public void FingerUp()
        {
            _parentTower.FingerUp();
        }
    }
}