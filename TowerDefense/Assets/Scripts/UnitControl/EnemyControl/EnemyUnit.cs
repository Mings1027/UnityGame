using System;
using DataControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Animator _anim;

        private SphereCollider _sphereCollider;

        private NavMeshAgent _navMeshAgent;
        private EnemyHealth _enemyHealth;
        private Collider[] _targetCollider;
        private AttackPoint _attackPoint;

        private Collider _target;
        private Cooldown atkCooldown;
        private bool _isTargeting;
        private bool _targetInAtkRange;

        private ushort _damage;
        private float atkRange;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        private static readonly int IsDead = Animator.StringToHash("isDead");

        public bool IsArrived { get; private set; }
        public event Action OnDecreaseLifeCountEvent;

        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _sphereCollider = GetComponent<SphereCollider>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            _attackPoint = GetComponentInChildren<AttackPoint>();
            atkRange = _sphereCollider.radius * 2f;
            _navMeshAgent.stoppingDistance = atkRange - 0.1f;
        }

        private void OnEnable()
        {
            IsArrived = false;
            _target = null;
            _isTargeting = false;
            _enemyHealth.OnDeadEvent += DeadAnimation;
            InvokeRepeating(nameof(Targeting), 0f, 0.5f);
        }

        private void Update()
        {
            if (_enemyHealth.IsDead) return;
            if (!_targetInAtkRange || !_isTargeting) return;
            _navMeshAgent.isStopped = true;
            if (atkCooldown.IsCoolingDown) return;
            Attack();
            atkCooldown.StartCooldown();
        }

        private void LateUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, !_navMeshAgent.isStopped);
        }

        private void OnDisable()
        {
            OnDecreaseLifeCountEvent = null;
            CancelInvoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
#endif
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        public void Targeting()
        {
            if (_enemyHealth.IsDead) return;
            if (!_navMeshAgent.isActiveAndEnabled) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (Vector3.Distance(Vector3.zero, transform.position) < 2)
            {
                IsArrived = true;
                _enemyHealth.Damage(0);
                if (!_enemyHealth.IsDead)
                {
                    OnDecreaseLifeCountEvent?.Invoke();
                }

                gameObject.SetActive(false);
                return;
            }

            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(Vector3.zero);
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
            }

            if (!_target) return;
            var targetPos = _target.transform.position;
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= atkRange;
            _navMeshAgent.SetDestination(targetPos);
        }

        private void Attack()
        {
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            if (_target.enabled) return;
            _target = null;
            _isTargeting = false;
        }

        public void SetAnimationSpeed(float animSpeed)
        {
            _anim.speed = animSpeed;
        }

        private void DeadAnimation()
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.enabled = false;
            _anim.SetTrigger(IsDead);
            DOVirtual.DelayedCall(2, () => gameObject.SetActive(false), false);
        }

        public void Init(EnemyData enemyData)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(_navMeshAgent.speed);
            TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.defaultSpeed = enemyData.Speed;
            atkCooldown.cooldownTime = enemyData.AttackDelay;
            _damage = enemyData.Damage;
        }
    }
}