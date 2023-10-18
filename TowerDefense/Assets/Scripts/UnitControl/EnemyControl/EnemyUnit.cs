using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Transform childMeshTransform;
        private AudioSource _audioSource;
        private Animator _anim;

        private NavMeshAgent _navMeshAgent;
        private EnemyHealth _enemyHealth;
        private Collider[] _targetCollider;

        private Collider _target;
        private bool _isTargeting;
        private bool _targetInAtkRange;
        private bool isAttacking;

        private ushort _damage;
        private float atkRange;
        private float atkDelay;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        #region Unity Event

        private void Awake()
        {
            childMeshTransform = transform.GetChild(0);
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            atkRange = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            _target = null;
            _isTargeting = false;
            isAttacking = false;
            _enemyHealth.OnDeadEvent += DeadAnimation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out IDamageable damageable)) return;
            _enemyHealth.DecreaseEnemyCount();
            damageable.Damage(1);
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
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
            _targetInAtkRange = Vector3.Distance(targetPos, transform.position) <= atkRange;
            _navMeshAgent.SetDestination(targetPos);
        }

        public async UniTaskVoid AttackAsync(CancellationTokenSource cts)
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, !_targetInAtkRange);
            if (!_targetInAtkRange || !_isTargeting || isAttacking) return;
            isAttacking = true;
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            isAttacking = false;
        }

        private void Attack()
        {
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            TryHit();
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
            DOTween.Sequence().Append(childMeshTransform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f))
                .Append(childMeshTransform.DOScale(0, 0.5f))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _navMeshAgent.enabled = true;
                    _anim.enabled = true;
                    childMeshTransform.rotation = Quaternion.identity;
                    childMeshTransform.localScale = Vector3.one;
                });
        }

        public void Init(EnemyData enemyData)
        {
            _navMeshAgent.SetDestination(Vector3.zero);
            _targetInAtkRange = false;
            _navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(_navMeshAgent.speed);
            atkDelay = enemyData.AttackDelay;
            _damage = enemyData.Damage;
        }
    }
}