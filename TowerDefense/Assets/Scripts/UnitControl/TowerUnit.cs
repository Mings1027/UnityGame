using System.Diagnostics;
using CustomEnumControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using ManagerControl;
using Plugins.Easy_performant_outline.Scripts;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public sealed class TowerUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private Sequence _deadSequence;
        private SummonTower _parentTower;
        private Collider _thisCollider;
        private Collider[] _targetCollider;
        private Health _health;
        private Collider _target;
        private Animator _anim;
        private NavMeshAgent _navMeshAgent;
        private Outlinable _outlinable;

        private LayerMask _targetLayer;
        private UnitState _unitState;
        private Vector3 _originPos;

        private Cooldown _atkCooldown;
        private int _damage;
        private bool _startTargeting;

        private readonly static int IsWalk = Animator.StringToHash("isWalk");
        private readonly static int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }

        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0, 5)] private byte attackTargetCount;
        [SerializeField, Range(0, 7)] private byte atkRange;
        [SerializeField, Range(0, 10)] private byte sightRange;

        #region Unity Event

        private void Awake()
        {
            _outlinable = GetComponent<Outlinable>();
            _outlinable.enabled = false;
            _targetLayer = LayerMask.GetMask("Monster");
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
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
                    ObjectDisable();
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
            _navMeshAgent.stoppingDistance = atkRange;
            _childMeshTransform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            _deadSequence?.Kill();
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, _navMeshAgent.destination) <=
                _navMeshAgent.stoppingDistance)
            {
                _navMeshAgent.stoppingDistance = atkRange;
                enabled = false;
                _anim.SetBool(IsWalk, false);
                return;
            }

            if (_unitState == UnitState.Chase)
            {
                enabled = false;
            }

            _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);
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

        #region Unit Update

        public void UnitUpdate()
        {
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

            // _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);

            if (_target && _target.enabled)
            {
                // var t = transform;
                // var targetRot = Quaternion.LookRotation(_target.transform.position - t.position);
                // t.rotation = Quaternion.Slerp(t.rotation, targetRot, turnSpeed);
                var dir = (_target.transform.position - transform.position).normalized;
                var targetRot = Quaternion.LookRotation(dir);
                var eulerAngleDiff = targetRot.eulerAngles - transform.rotation.eulerAngles;
                transform.Rotate(eulerAngleDiff);
            }
        }

        private void Patrol()
        {
            _anim.SetBool(IsWalk, false);
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, _targetLayer);
            if (size <= 0)
            {
                _target = null;
                if (Vector3.Distance(_originPos, transform.position) > 1)
                {
                    Move(_originPos);
                }

                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _targetCollider[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    _target = _targetCollider[i];
                }
            }

            _unitState = UnitState.Chase;
        }

        private void Chase()
        {
            _anim.SetBool(IsWalk, true);

            if (!_target.enabled)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            _anim.SetBool(IsWalk, true);
            if (_navMeshAgent.isOnNavMesh)
                _navMeshAgent.SetDestination(_target.transform.position + Random.insideUnitSphere * atkRange);
            if (Vector3.Distance(_target.transform.position, transform.position) <= atkRange)
            {
                _unitState = UnitState.Attack;
            }
        }

        private void Attack()
        {
            _anim.SetBool(IsWalk,false);
            if (!_target || !_target.enabled ||
                Vector3.Distance(_target.transform.position, transform.position) > atkRange)
            {
                _unitState = UnitState.Patrol;
                return;
            }

            if (_atkCooldown.IsCoolingDown) return;

            SoundManager.Play3DSound(audioClip, transform.position);
            _anim.SetTrigger(IsAttack);
            TryDamage();

            _atkCooldown.StartCooldown();
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

        private void ObjectDisable()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Private Method

        public void DisableObject()
        {
            _parentTower = null;
            _thisCollider.enabled = false;
        }

        #endregion

        #region Public Method

        public void Init(SummonTower summonTower, Vector3 pos)
        {
            _originPos = pos;
            _parentTower = summonTower;
            _anim.enabled = true;
            _thisCollider.enabled = true;
            _target = null;
            _health.OnDeadEvent += Dead;
            _navMeshAgent.enabled = true;
        }

        public void UnitTargetInit()
        {
            Move(_originPos);
            _unitState = UnitState.Patrol;
            _target = null;
        }

        public void Move(Vector3 pos)
        {
            enabled = true;
            _anim.enabled = true;
            _originPos = pos;
            _anim.SetBool(IsWalk, true);
            _navMeshAgent.stoppingDistance = 0.1f;
            if (_navMeshAgent.isOnNavMesh)
                _navMeshAgent.SetDestination(pos);
        }

        public void ActiveIndicator() => _outlinable.enabled = true;

        public void DeActiveIndicator() => _outlinable.enabled = false;

        public void UnitUpgrade(int damage, int healthAmount, float attackDelayData)
        {
            _damage = damage;
            _health.Init(healthAmount);
            _atkCooldown.cooldownTime = attackDelayData;
        }

        public void ParentPointerUp() => _parentTower.OnPointerUp(null);

        #endregion
    }
}