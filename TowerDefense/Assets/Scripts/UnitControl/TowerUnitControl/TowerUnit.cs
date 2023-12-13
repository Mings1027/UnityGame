using System.Diagnostics;
using CustomEnumControl;
using DG.Tweening;
using EPOOutline;
using GameControl;
using InterfaceControl;
using StatusControl;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace UnitControl.TowerUnitControl
{
    public sealed class TowerUnit : MonoBehaviour
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

        private LayerMask _targetLayer;
        private UnitState _unitState;
        private Vector3 _originPos;

        private Cooldown _cooldown;
        private int _damage;
        private bool _moveInput;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }

        [SerializeField, Range(0, 5)] private byte attackTargetCount;
        [SerializeField, Range(0, 7)] private float atkRange;
        [SerializeField, Range(0, 10)] private float sightRange;

        public Outlinable outline { get; private set; }

        #region Unity Event

        private void Awake()
        {
            _cooldown = new Cooldown();
            _targetLayer = LayerMask.GetMask("Monster");
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
                    ObjectDisable();
                    _childMeshTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    transform.localScale = Vector3.one;
                });
            outline = GetComponent<Outlinable>();
            outline.enabled = false;
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
        }

        private void OnDisable()
        {
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
                _moveInput = false;
                _navMeshAgent.stoppingDistance = atkRange;
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

            _anim.SetBool(IsWalk, _navMeshAgent.velocity != Vector3.zero);
        }

        private void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, _targetLayer);
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
                _anim.enabled = false;
                return;
            }

            if (_cooldown.IsCoolingDown) return;

            var t = transform;
            var targetRot = Quaternion.LookRotation(_target.transform.position - t.position);
            targetRot.eulerAngles = new Vector3(0, targetRot.eulerAngles.y, targetRot.eulerAngles.z);
            t.rotation = targetRot;
            // transform.forward = _target.bounds.center - transform.position;
            
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryDamage();

            _cooldown.StartCooldown();
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

        #region Public Mothod

        public void Init()
        {
            _thisCollider.enabled = true;
            _target = null;
            _health.OnDeadEvent += Dead;
            _navMeshAgent.enabled = true;
            _anim.enabled = true;
        }

        public void UnitTargetInit()
        {
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
            if (_navMeshAgent.isOnNavMesh)
                _navMeshAgent.SetDestination(pos);
        }

        public void ActiveIndicator() => _parentTower.OnPointerUp(null);

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
            _cooldown.cooldownTime = attackDelayData;
        }

        #endregion
    }
}