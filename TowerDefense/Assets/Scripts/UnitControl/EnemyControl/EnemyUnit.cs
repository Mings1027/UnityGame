using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private CancellationTokenSource cts;
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
            cts?.Dispose();
            cts = new CancellationTokenSource();
            _target = null;
            _isTargeting = false;
            _enemyHealth.OnDeadEvent += () => DeadAnimation().Forget();
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

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("BaseTower")) return;

            _enemyHealth.DecreaseEnemyCount();
            if (!_enemyHealth.IsDead)
            {
                OnDecreaseLifeCountEvent?.Invoke();
            }

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            cts?.Cancel();
            cts?.Dispose();
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

        private async UniTaskVoid DeadAnimation()
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.enabled = false;
            _anim.SetTrigger(IsDead);
            await UniTask.Delay(2000, cancellationToken: cts.Token);
            gameObject.SetActive(false);
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