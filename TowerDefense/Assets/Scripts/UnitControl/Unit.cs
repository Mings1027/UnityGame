using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private AudioSource _audioSource;
        private Collider _collider;
        private Collider[] _targetCollider;
        private bool _isAttacking;

        protected Health health;
        protected Collider target;
        protected Animator anim;
        protected NavMeshAgent navMeshAgent;

        protected UnitState unitState;

        protected int damage;
        protected float atkDelay;
        protected float atkRange;

        protected static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;
        
        protected virtual void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            _audioSource = GetComponent<AudioSource>();
            anim = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            _collider = GetComponent<Collider>();
            health = GetComponent<Health>();
            _targetCollider = new Collider[1];
            atkRange = navMeshAgent.stoppingDistance;
        }

        private void OnDisable()
        {
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
            _childMeshTransform.rotation = Quaternion.identity;
            _childMeshTransform.localScale = Vector3.one;
        }

        public void UnitUpdate(CancellationTokenSource cts)
        {
            switch (unitState)
            {
                case UnitState.Patrol:
                    Patrol();
                    break;
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    ReadyToAttack(cts).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0) return;
            target = _targetCollider[0];
            unitState = UnitState.Chase;
        }

        private void Chase()
        {
            if (!target.enabled)
            {
                unitState = UnitState.Patrol;
            }

            anim.SetBool(IsWalk, true);
            navMeshAgent.SetDestination(target.transform.position);
            if (Vector3.Distance(target.transform.position, transform.position) <= atkRange)
            {
                unitState = UnitState.Attack;
                anim.SetBool(IsWalk, false);
            }
        }

        private async UniTaskVoid ReadyToAttack(CancellationTokenSource cts)
        {
            if (Vector3.Distance(target.transform.position, transform.position) > atkRange || !target.enabled)
            {
                unitState = UnitState.Patrol;
                _isAttacking = false;
            }

            if (_isAttacking) return;
            _isAttacking = true;
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            _isAttacking = false;
        }

        protected virtual void Attack()
        {
            transform.forward = target.transform.position - transform.position;
            anim.SetTrigger(IsAttack);
            _audioSource.Play();
            if (target.enabled && target.TryGetComponent(out IDamageable damageable)) damageable.Damage(damage);
        }

        private void Dead()
        {
            _collider.enabled = false;
            navMeshAgent.isStopped = true;
            anim.enabled = false;
            DOTween.Sequence().Append(_childMeshTransform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f))
                .Append(_childMeshTransform.DOScale(0, 0.5f))
                .OnComplete(() => { gameObject.SetActive(false); });
        }

        public void Init()
        {
            _collider.enabled = true;
            target = null;
            _isAttacking = false;
            health.OnDeadEvent += Dead;
            navMeshAgent.isStopped = false;
            anim.enabled = true;
        }
    }
}