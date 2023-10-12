using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Transform childMeshTransform;
        private CancellationTokenSource cts;
        private AudioSource _audioSource;
        private Animator _anim;

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
      
        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            childMeshTransform = transform.GetChild(0);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            _attackPoint = GetComponentInChildren<AttackPoint>();

            atkRange = _navMeshAgent.stoppingDistance;
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

            if (atkCooldown.IsCoolingDown) return;
            Attack();
            atkCooldown.StartCooldown();
        }

        private void LateUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, !_targetInAtkRange);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out IDamageable damageable)) return;
            _enemyHealth.DecreaseEnemyCount();
            damageable.Damage(1);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            cts?.Cancel();
            cts?.Dispose();
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
                _targetInAtkRange = false;
                _target = null;
                _isTargeting = false;
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
            _navMeshAgent.enabled = false;
            _anim.enabled = false;
            await childMeshTransform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f).SetEase(Ease.Linear);
            await UniTask.Delay(500, cancellationToken: cts.Token);
            await childMeshTransform.DOScale(0, 0.5f).SetEase(Ease.Linear);
            gameObject.SetActive(false);
            _anim.enabled = true;
            childMeshTransform.rotation = Quaternion.identity;
            childMeshTransform.localScale = Vector3.one;
        }

        public void Init(EnemyData enemyData)
        {
            _targetInAtkRange = false;
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