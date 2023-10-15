using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.FriendlyControl
{
    public sealed class FriendlyUnit : MonoBehaviour, IFingerUp
    {
        private CancellationTokenSource cts;
        private Transform childMeshTransform;
        private AudioSource _audioSource;
        private Animator _anim;
        private UnitNavAI _unitNavAI;
        private NavMeshAgent _navMeshAgent;

        private Health _health;
        private Collider[] _targetCollider;
        private UnitTower _parentTower;
        private AttackPoint _attackPoint;
        private Cooldown atkCooldown;
        private Collider _target;

        private TowerType _towerTypeEnum;
        private ushort _damage;

        private bool _startTargeting;
        private bool _isTargeting;
        private bool _moveInput;
        private bool _targetInAtkRange;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

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
            childMeshTransform = transform.GetChild(0);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _unitNavAI = GetComponent<UnitNavAI>();
            _unitNavAI.enabled = false;
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _targetCollider = new Collider[2];
            _attackPoint = GetComponentInChildren<AttackPoint>();
            atkRange = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            UnitTargetInit();
            _health.OnDeadEvent += () => DeadAnimation().Forget();
            indicator.enabled = false;
        }

        private void OnDisable()
        {
            _parentTower = null;
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
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

        public void UnitTargetInit()
        {
            _startTargeting = false;
            _target = null;
            _isTargeting = false;
            _targetInAtkRange = false;
            if (_moveInput) return;
            _anim.SetBool(IsWalk, false);
        }

        public void UnitUpdate()
        {
            if (_health.IsDead) return;
            _anim.SetBool(IsWalk, _moveInput || !_navMeshAgent.velocity.Equals(Vector3.zero));

            if (!_targetInAtkRange || !_isTargeting) return;
            if (atkCooldown.IsCoolingDown) return;
            Attack();
            atkCooldown.StartCooldown();
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
            if (_moveInput) return;
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            DataManager.SumDamage(_towerTypeEnum, _damage);
        }

        private async UniTaskVoid DeadAnimation()
        {
            _navMeshAgent.enabled = false;
            _anim.enabled = false;
            await childMeshTransform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f).SetEase(Ease.Linear);
            await UniTask.Delay(500, cancellationToken: cts.Token);
            await childMeshTransform.DOScale(0, 0.5f).SetEase(Ease.Linear);
            gameObject.SetActive(false);
            _navMeshAgent.enabled = true;
            _anim.enabled = true;
            childMeshTransform.rotation = Quaternion.identity;
            childMeshTransform.localScale = Vector3.one;
        }

        public async UniTask MoveToTouchPos(Vector3 pos)
        {
            _moveInput = true;
            _anim.SetBool(IsWalk, true);
            _navMeshAgent.isStopped = false;
            _navMeshAgent.stoppingDistance = 0.1f;
            _navMeshAgent.SetDestination(pos);
            _unitNavAI.enabled = true;
            await UniTask.WaitUntil(_unitNavAI.isStopped);
            _navMeshAgent.stoppingDistance = atkRange;
            _moveInput = false;
            _anim.SetBool(IsWalk, false);
            if (_startTargeting) return;
            _unitNavAI.enabled = false;
        }

        public void SpawnInit(UnitTower unitTower, TowerType towerTypeEnum)
        {
            _parentTower = unitTower;
            _towerTypeEnum = towerTypeEnum;
        }

        public void UnitUpgrade(ushort damage, int healthAmount, float delay)
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