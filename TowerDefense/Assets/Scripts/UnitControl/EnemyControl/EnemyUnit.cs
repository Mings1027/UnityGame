using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Sequence _deadSequence;

        private bool _isAttacking;
        private Collider _thisCollider;
        protected Collider[] targetCollider;
        private Health _health;
        private Collider _target;
        private Animator _anim;
        private NavMeshAgent _navMeshAgent;

        private UnitState _unitState;

        protected int damage;
        private float _atkDelay;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField, Range(1, 5)] private byte attackTargetCount;
        [SerializeField, Range(1, 7)] private float atkRange;
        [SerializeField, Range(1, 10)] private float sightRange;
        [SerializeField] private LayerMask targetLayer;
        private Vector3 _prevPos;

        #region Unity Event

        private void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _thisCollider = GetComponent<Collider>();
            _health = GetComponent<Health>();
            targetCollider = new Collider[attackTargetCount];

            _deadSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_childMeshTransform.DOLocalJump(-_childMeshTransform.forward, Random.Range(4, 7), 1, 1))
                .Join(_childMeshTransform.DOLocalRotate(new Vector3(-360, 0, 0), 1, RotateMode.FastBeyond360))
                .Join(transform.DOScale(0, 1).From(transform.localScale))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _childMeshTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    transform.localScale = Vector3.one;
                });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("BaseTower")) return;
            _thisCollider.enabled = false;
            gameObject.SetActive(false);
        }

        protected virtual void OnDisable()
        {
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
            _childMeshTransform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            _deadSequence?.Kill();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        #endregion

        #region Unit Update

        public void EnemyTargeting()
        {
            if (!_navMeshAgent.enabled) return;
            if (_health.IsDead) return;
            Patrol();
        }

        public void UnitUpdate(CancellationTokenSource cts)
        {
            if (!_navMeshAgent.enabled) return;
            if (_health.IsDead) return;
            switch (_unitState)
            {
                // case UnitState.Patrol:
                //     Patrol();
                //     break;
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    Attack(cts).Forget();
                    break;
                // default:
                //     throw new ArgumentOutOfRangeException();
            }

            _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);
        }

        private void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, targetCollider, targetLayer);
            if (size <= 0)
            {
                if (_navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.SetDestination(Vector3.zero);
                    return;
                }
            }

            _target = targetCollider[0];
            _unitState = UnitState.Chase;
        }

        private void Chase()
        {
            if (!_target.enabled)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            if (_navMeshAgent.isOnNavMesh)
                _navMeshAgent.SetDestination(_target.transform.position);
            if (Vector3.Distance(_target.transform.position, transform.position) <= atkRange)
            {
                _unitState = UnitState.Attack;
            }
        }

        private async UniTaskVoid Attack(CancellationTokenSource cts)
        {
            if (_isAttacking) return;
            _isAttacking = true;
            var t = transform;
            t.Rotate(_target.transform.position - t.position);
            _anim.SetTrigger(IsAttack);
            // _audioSource.Play();
            TryDamage();
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            _isAttacking = false;

            if (!_target || !_target.enabled ||
                Vector3.Distance(_target.transform.position, transform.position) > atkRange)
            {
                _unitState = UnitState.Patrol;
            }
        }

        protected virtual void TryDamage()
        {
            if (_target.enabled && _target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(damage);
        }

        private void Dead()
        {
            _thisCollider.enabled = false;
            _navMeshAgent.enabled = false;
            _anim.enabled = false;
            _deadSequence.Restart();
        }

        public void Init()
        {
            _thisCollider.enabled = true;
            _target = null;
            _isAttacking = false;
            _health.OnDeadEvent += Dead;
            _navMeshAgent.enabled = true;
            _anim.enabled = true;
        }

        #endregion

        public void SetAnimationSpeed(float animSpeed)
        {
            _anim.speed = animSpeed;
        }

        public void SpawnInit(EnemyData enemyData)
        {
            _prevPos = transform.position;
            _unitState = UnitState.Patrol;
            _navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(_navMeshAgent.speed);
            _atkDelay = enemyData.AttackDelay;
            damage = enemyData.Damage;
            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.SetDestination(Vector3.zero);
            _anim.SetBool(IsWalk, true);
        }

        public async UniTaskVoid IfStuck(CancellationTokenSource cts)
        {
            if (_unitState == UnitState.Patrol && Vector3.Distance(_prevPos, transform.position) < 1.5f)
            {
                _navMeshAgent.enabled = false;
                await UniTask.Delay(500, cancellationToken: cts.Token);
                if (!gameObject.activeSelf) return;
                _navMeshAgent.enabled = true;
                _navMeshAgent.ResetPath();
                if (_navMeshAgent.isOnNavMesh) _navMeshAgent.SetDestination(Vector3.zero);
                _prevPos = transform.position;
            }
        }
    }
}