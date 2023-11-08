using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Animator _anim;
        private NavMeshAgent _navMeshAgent;
        private Collider[] _targetCollider;
        private Collider _target;
        private Health _enemyHealth;
        private bool _isTargeting;
        private bool _targetInAtkRange;
        private bool _isAttacking;

        private ushort _damage;
        private float _atkDelay;
        private float _atkRange;

        public Vector3 prevPos { get; private set; }
        public event Action OnArrivedBaseTower;
        public event Action OnDisableEvent;
        public Transform HealthBarTransform { get; private set; }

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        #region Unity Event

        private void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            HealthBarTransform = transform.GetChild(1);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyHealth = GetComponent<Health>();
            _targetCollider = new Collider[1];
            _atkRange = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            _target = null;
            _isTargeting = false;
            _isAttacking = false;
            _enemyHealth.OnDeadEvent += DeadAnimation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BaseTower"))
            {
                StatusInit();
                OnArrivedBaseTower?.Invoke();
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            OnArrivedBaseTower = null;
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _atkRange);
        }
#endif

        #endregion

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

            _isTargeting = _targetCollider[0].enabled;
            _target = _isTargeting ? _targetCollider[0] : null;

            if (!_target) return;
            var targetPos = _target.transform.position;
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= _atkRange;
            _navMeshAgent.SetDestination(targetPos);
        }

        public async UniTaskVoid AttackAsync(CancellationTokenSource cts)
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, !_targetInAtkRange);
            if (!_targetInAtkRange || !_isTargeting || _isAttacking) return;
            _isAttacking = true;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryHit();
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        private void TryHit()
        {
            if (!_target) return;
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
            }
        }

        public void SetAnimationSpeed(float animSpeed)
        {
            _anim.speed = animSpeed;
        }

        private void DeadAnimation()
        {
            _navMeshAgent.enabled = false;
            _anim.enabled = false;
            DOTween.Sequence().Append(_childMeshTransform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f))
                .Append(_childMeshTransform.DOScale(0, 0.5f))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    StatusInit();
                });
        }

        public async UniTaskVoid ResetNavmesh()
        {
            if (!gameObject.activeSelf) return;
            prevPos = transform.position;
            _navMeshAgent.enabled = false;
            await UniTask.Yield();
            if (!gameObject.activeSelf) return;
            _navMeshAgent.enabled = true;
            _navMeshAgent.SetDestination(Vector3.zero);
        }

        public void StatusInit()
        {
            _navMeshAgent.enabled = true;
            _anim.enabled = true;
            _childMeshTransform.rotation = Quaternion.identity;
            _childMeshTransform.localScale = Vector3.one;
        }

        public void Init(EnemyData enemyData)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = false;
            _targetInAtkRange = false;
            _navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(_navMeshAgent.speed);
            _atkDelay = enemyData.AttackDelay;
            _damage = enemyData.Damage;
            _navMeshAgent.SetDestination(Vector3.zero);
        }
    }
}