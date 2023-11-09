using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EPOOutline;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Animator _anim;
        private UnitNavAI _unitNavAI;
        private NavMeshAgent _navMeshAgent;

        private Health _health;
        private Collider[] _targetCollider;
        private UnitTower _parentTower;

        private float _attackDelay;
        private Collider _target;

        private TowerType _towerTypeEnum;
        private int _damage;

        private bool _startTargeting;
        private bool _isTargeting;
        private bool _moveInput;
        private bool _targetInAtkRange;
        private bool _isAttacking;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Outlinable outline { get; private set; }
        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField] private LayerMask targetLayer;
        private float _atkRange;
        [SerializeField] private float sightRange;

        #region Unity Event

        private void Awake()
        {
            outline = GetComponent<Outlinable>();
            outline.enabled = false;
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _unitNavAI = GetComponent<UnitNavAI>();
            _unitNavAI.enabled = false;
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.enabled = false;
            _health = GetComponent<Health>();
            _targetCollider = new Collider[2];
            _atkRange = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            UnitTargetInit();
            _health.OnDeadEvent += DeadAnimation;
        }

        private void OnDisable()
        {
            _parentTower = null;
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _atkRange);
        }
#endif

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            _parentTower.OnPointerUp(null);
        }

        #endregion

        public void UnitTargetInit()
        {
            _startTargeting = false;
            _target = null;
            _isTargeting = false;
            _isAttacking = false;
            _targetInAtkRange = false;
            if (_moveInput) return;
            _anim.SetBool(IsWalk, false);
        }

        public void UnitTargeting()
        {
            if (_moveInput) return;
            if (!_navMeshAgent.isActiveAndEnabled) return;
            _startTargeting = true;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _targetInAtkRange = false;
                _target = null;
                _isTargeting = false;
                return;
            }

            _isTargeting = _targetCollider[0];
            _target = _isTargeting ? _targetCollider[0] : null;

            if (!_target) return;
            var targetPos = _target.transform.position;
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= _atkRange;
            _navMeshAgent.SetDestination(targetPos);
        }

        public void UnitAnimation()
        {
            if (_moveInput || _health.IsDead) return;
            _anim.SetBool(IsWalk, !_navMeshAgent.velocity.Equals(Vector3.zero));
        }

        public async UniTaskVoid UnitAttackAsync(CancellationTokenSource cts)
        {
            if (_health.IsDead) return;

            if (_moveInput || !_targetInAtkRange || !_isTargeting || _isAttacking) return;
            _isAttacking = true;
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        private void Attack()
        {
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryHit();
            DataManager.SumDamage(_towerTypeEnum, _damage);
        }

        private void TryHit()
        {
            if (!_target) return;
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
            }
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
                    _navMeshAgent.enabled = true;
                    _anim.enabled = true;
                    _childMeshTransform.rotation = Quaternion.identity;
                    _childMeshTransform.localScale = Vector3.one;
                });
        }

        public async UniTask MoveToTouchPos(Vector3 pos)
        {
            _moveInput = true;
            _targetInAtkRange = false;
            _isTargeting = false;
            _anim.SetBool(IsWalk, true);
            _navMeshAgent.isStopped = false;
            _navMeshAgent.stoppingDistance = 0.1f;
            _navMeshAgent.SetDestination(pos);
            _unitNavAI.enabled = true;
            await UniTask.WaitUntil(_unitNavAI.isStopped);
            _navMeshAgent.stoppingDistance = _atkRange;
            _moveInput = false;
            _anim.SetBool(IsWalk, false);
            if (_startTargeting) return;
            _unitNavAI.enabled = false;
        }

        public void SpawnInit(UnitTower unitTower, TowerType towerTypeEnum)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = false;
            _parentTower = unitTower;
            _towerTypeEnum = towerTypeEnum;
        }

        public void UnitUpgrade(int damage, int healthAmount, float attackDelayData)
        {
            _damage = damage;
            _health.Init(healthAmount);
            _attackDelay = attackDelayData;
        }
    }
}