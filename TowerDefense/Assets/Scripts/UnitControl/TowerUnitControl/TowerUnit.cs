using System;
using System.Diagnostics;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EPOOutline;
using InterfaceControl;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace UnitControl.TowerUnitControl
{
    public sealed class TowerUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Sequence _deadSequence;

        private UnitTower _parentTower;
        private Collider _thisCollider;
        private Collider[] _targetCollider;
        private Health _health;
        private Collider _target;
        private Animator _anim;
        private NavMeshAgent _navMeshAgent;

        private UnitState _unitState;

        private int _damage;
        private float _atkDelay;

        private Vector3 _originPos;
        private bool _moveInput;
        private bool _isAttacking;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField, Range(1, 5)] private byte attackTargetCount;
        [SerializeField, Range(1, 7)] private float atkRange;
        [SerializeField, Range(1, 10)] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        public Outlinable outline { get; private set; }

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
            _targetCollider = new Collider[attackTargetCount];

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
            outline = GetComponent<Outlinable>();
            outline.enabled = false;
        }

        private void OnDisable()
        {
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
            _childMeshTransform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            _deadSequence?.Kill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            _parentTower.OnPointerUp(null);
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, _navMeshAgent.destination) <=
                _navMeshAgent.stoppingDistance)
            {
                _moveInput = false;
                _navMeshAgent.stoppingDistance = 1;
                _anim.SetBool(IsWalk, false);
                _anim.enabled = false;
                enabled = false;
            }

            if (_unitState == UnitState.Chase)
            {
                enabled = false;
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        #endregion

        #region Unit Update

        public void UnitTargeting()
        {
            if (_health.IsDead) return;
            Patrol();
        }

        public void UnitUpdate(CancellationTokenSource cts)
        {
            if (_health.IsDead) return;
            switch (_unitState)
            {
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    Attack(cts).Forget();
                    break;
            }

            _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);
        }

        private void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                if (Vector3.Distance(_originPos, transform.position) > 2)
                {
                    Move(_originPos);
                }

                return;
            }

            _target = _targetCollider[0];
            _anim.enabled = true;
            _unitState = UnitState.Chase;
        }

        private void Chase()
        {
            if (!_target.enabled)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.SetDestination(_target.transform.position);
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
            transform.rotation = Quaternion.LookRotation(_target.transform.position - t.position);
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryDamage();
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            _isAttacking = false;

            if (!_target || !_target.enabled ||
                Vector3.Distance(_target.transform.position, transform.position) > atkRange)
            {
                _unitState = UnitState.Patrol;
                _anim.enabled = false;
            }
        }

        private void TryDamage()
        {
            if (_target.enabled && _target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(_damage);
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

        #region Public Mothod

        public void UnitTargetInit()
        {
            _isAttacking = false;
            Move(_originPos);
            _unitState = UnitState.Patrol;
            _target = null;
            if (_moveInput) return;
            _anim.SetBool(IsWalk, false);
        }

        public void Move(Vector3 pos)
        {
            enabled = true;
            _anim.enabled = true;
            _originPos = pos;
            _moveInput = true;
            _anim.SetBool(IsWalk, true);
            _navMeshAgent.stoppingDistance = 0.1f;
            _navMeshAgent.SetDestination(pos);
        }

        public void InfoInit(UnitTower unitTower, Vector3 pos)
        {
            _originPos = pos;
            _parentTower = unitTower;
        }

        public void DisableParent()
        {
            _parentTower = null;
        }

        public void UnitUpgrade(int damage, int healthAmount, float attackDelayData)
        {
            _damage = damage;
            _health.Init(healthAmount);
            _atkDelay = attackDelayData;
        }

        #endregion
    }
}