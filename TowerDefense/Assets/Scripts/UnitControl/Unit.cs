using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Sequence _deadSequence;

        protected bool isAttacking;
        protected Collider thisCollider;
        protected Collider[] targetCollider;
        protected Health health;
        protected Collider target;
        protected Animator anim;
        protected NavMeshAgent navMeshAgent;

        protected UnitState unitState;

        protected int damage;
        protected float atkDelay;

        protected static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField, Range(1, 5)] private byte attackTargetCount;
        [SerializeField, Range(1, 7)] private float atkRange;
        [SerializeField, Range(1, 10)] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        #region Unity Event

        protected virtual void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            _audioSource = GetComponent<AudioSource>();
            anim = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            thisCollider = GetComponent<Collider>();
            health = GetComponent<Health>();
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
#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = unitState == UnitState.Attack ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
#endif

        #endregion

        public virtual void UnitUpdate(CancellationTokenSource cts)
        {
            if (health.IsDead) return;
            switch (unitState)
            {
                case UnitState.Patrol:
                    Patrol();
                    break;
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    Attack(cts).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            anim.SetBool(IsWalk, navMeshAgent.velocity != Vector3.zero);
        }

        protected virtual void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, targetCollider, targetLayer);
            if (size <= 0) return;
            target = targetCollider[0];
            unitState = UnitState.Chase;
        }

        private void Chase()
        {
            if (!target.enabled)
            {
                unitState = UnitState.Patrol;
                return;
            }

            navMeshAgent.SetDestination(target.transform.position);
            if (Vector3.Distance(target.transform.position, transform.position) <= atkRange)
            {
                unitState = UnitState.Attack;
            }
        }

        private async UniTaskVoid Attack(CancellationTokenSource cts)
        {
            if (isAttacking) return;
            isAttacking = true;
            var t = transform;
            t.Rotate(target.transform.position - t.position);
            anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryDamage();
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            isAttacking = false;

            if (!target || !target.enabled ||
                Vector3.Distance(target.transform.position, transform.position) > atkRange)
            {
                unitState = UnitState.Patrol;
            }
        }

        protected virtual void TryDamage()
        {
            if (target.enabled && target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(damage);
        }

        protected virtual void Dead()
        {
            thisCollider.enabled = false;
            navMeshAgent.enabled = false;
            anim.enabled = false;
            _deadSequence.Restart();
        }

        public void Init()
        {
            thisCollider.enabled = true;
            target = null;
            isAttacking = false;
            health.OnDeadEvent += Dead;
            navMeshAgent.enabled = true;
            anim.enabled = true;
        }
    }
}