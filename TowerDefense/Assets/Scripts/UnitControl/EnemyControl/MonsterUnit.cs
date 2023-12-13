using System;
using System.Diagnostics;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace UnitControl.EnemyControl
{
    public class MonsterUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private Sequence _deadSequence;
        private Collider _thisCollider;
        private Health _health;
        private Collider _target;
        private Animator _anim;
        private NavMeshAgent _navMeshAgent;

        private LayerMask _targetLayer;
        private UnitState _unitState;
        private Cooldown _attackCooldown;
        private Cooldown _patrolCooldown;

        protected Collider[] targetCollider;
        protected int damage;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField, Range(0, 5)] private byte attackTargetCount;
        [SerializeField, Range(0, 7)] private float atkRange;
        [SerializeField, Range(0, 10)] private float sightRange;
        [SerializeField] private byte baseOffset;

        #region Unity Event

        private void Awake()
        {
            _targetLayer = LayerMask.GetMask("Unit");
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
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
                    DisableObject();
                    _childMeshTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    transform.localScale = Vector3.one;
                });
        }

        private void OnValidate()
        {
            if (atkRange > sightRange)
            {
                Debug.LogError("atkRange must be smaller than Sight Range");
                atkRange = sightRange;
            }
        }

        private void OnEnable()
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.stoppingDistance = atkRange;
        }

        private void OnTriggerEnter(Collider other)
        {
            DisableObject();
        }

        private void OnDestroy()
        {
            _deadSequence?.Kill();
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var position = transform.position;
            Gizmos.DrawWireSphere(position, atkRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, sightRange);
        }

        #endregion

        #region Init

        public void Init()
        {
            _thisCollider.enabled = true;
            _target = null;
            _health.OnDeadEvent += Dead;
            _anim.enabled = true;
            _patrolCooldown.cooldownTime = 0.5f;
        }

        public void SpawnInit(MonsterData monsterData)
        {
            _unitState = UnitState.Patrol;
            _navMeshAgent.speed = monsterData.Speed;
            SetSpeed(_navMeshAgent.speed, _attackCooldown.cooldownTime);
            _attackCooldown.cooldownTime = monsterData.AttackDelay;
            damage = monsterData.Damage;
            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.SetDestination(Vector3.zero);
            _anim.SetBool(IsWalk, true);
            SetBaseOffset().Forget();
        }

        private async UniTaskVoid SetBaseOffset()
        {
            if (baseOffset == 0) return;
            var lerp = 0f;
            while (_navMeshAgent.baseOffset < baseOffset)
            {
                await UniTask.Delay(10);
                lerp += Time.deltaTime;
                var offset = Mathf.Lerp(0, baseOffset, lerp);
                _navMeshAgent.baseOffset = offset;
            }
        }

        #endregion

        #region Unit Update

        public void MonsterUpdate()
        {
            if (!_navMeshAgent.enabled) return;
            if (_health.IsDead) return;
            switch (_unitState)
            {
                case UnitState.Patrol:
                    Patrol();
                    break;
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    Attack();
                    break;
            }

            _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);
        }

        #region Monster State

        private void Patrol()
        {
            if (_patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, targetCollider, _targetLayer);
            if (size <= 0)
            {
                if (_navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.SetDestination(Vector3.zero);
                }

                return;
            }

            _target = targetCollider[0];
            _unitState = UnitState.Chase;
            _patrolCooldown.StartCooldown();
        }

        private void Chase()
        {
            if (!_target.enabled)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            if (_navMeshAgent.isOnNavMesh)
                _navMeshAgent.SetDestination(_target.transform.position + Random.insideUnitSphere * atkRange);
            if (Vector3.Distance(_target.transform.position, transform.position) <= atkRange)
            {
                _unitState = UnitState.Attack;
            }
        }

        private void Attack()
        {
            if (!_target || !_target.enabled ||
                Vector3.Distance(_target.transform.position, transform.position) > atkRange)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            if (_attackCooldown.IsCoolingDown) return;

            var t = transform;
            var targetRot = Quaternion.LookRotation(_target.transform.position - t.position);
            targetRot.eulerAngles = new Vector3(0, targetRot.eulerAngles.y, targetRot.eulerAngles.z);
            t.rotation = targetRot;

            _anim.SetTrigger(IsAttack);
            TryDamage();

            _attackCooldown.StartCooldown();
        }

        protected virtual void TryDamage()
        {
            if (_target.enabled && _target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(damage);
        }

        #endregion

        private void Dead()
        {
            _thisCollider.enabled = false;
            _navMeshAgent.enabled = false;
            _anim.enabled = false;
            _deadSequence.Restart();
        }

        #endregion

        protected virtual void DisableObject()
        {
            _navMeshAgent.baseOffset = 0;
            _navMeshAgent.enabled = false;
            _thisCollider.enabled = false;
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
            gameObject.SetActive(false);
        }

        public void SetSpeed(float animSpeed, float atkDelay)
        {
            _navMeshAgent.speed = animSpeed;
            _anim.speed = animSpeed;
            _attackCooldown.cooldownTime = atkDelay;
        }
    }
}