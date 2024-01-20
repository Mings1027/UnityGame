using System;
using System.Diagnostics;
using CustomEnumControl;
using DataControl.MonsterDataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MonsterControl
{
    public abstract class MonsterUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private Sequence _deadSequence;
        private Collider _thisCollider;

        protected Animator anim;
        protected Cooldown attackCooldown;
        protected Cooldown patrolCooldown;
        protected Collider target;
        protected Collider[] targetCollider;
        protected LayerMask targetLayer;
        protected UnitState unitState;

        protected Health health;
        protected NavMeshAgent navMeshAgent;
        protected int damage;

        private readonly int _isWalk = Animator.StringToHash("isWalk");
        protected readonly int isAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField, Range(0, 5)] protected byte attackTargetCount;
        [SerializeField, Range(0, 7)] protected byte atkRange;
        [SerializeField, Range(0, 10)] protected byte sightRange;
        [field: SerializeField] public byte baseTowerDamage { get; private set; }

#region Unity Event

        protected virtual void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            anim = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            _thisCollider = GetComponent<Collider>();
            health = GetComponent<Health>();

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

        private void OnDestroy()
        {
            _deadSequence?.Kill();
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

#endregion

#region Init

        public virtual void Init()
        {
            _thisCollider.enabled = true;
            target = null;
            health.OnDeadEvent += Dead;
            anim.enabled = true;
        }

        public virtual void SpawnInit(MonsterData monsterData)
        {
            navMeshAgent.enabled = true;
            unitState = UnitState.Patrol;
            navMeshAgent.speed = monsterData.Speed;
            SetSpeed(navMeshAgent.speed, attackCooldown.cooldownTime);
            attackCooldown.cooldownTime = monsterData.AttackDelay;
            damage = monsterData.Damage;
            if (navMeshAgent.isOnNavMesh) navMeshAgent.SetDestination(Vector3.zero);
            anim.SetBool(_isWalk, true);
        }

#endregion

#region Unit Update

        public virtual void MonsterUpdate()
        {
            anim.SetBool(_isWalk, navMeshAgent.velocity != Vector3.zero);
        }

#region Monster State

        protected virtual void Patrol()
        {
        }

        protected virtual void Attack()
        {
        }

        protected virtual void TryDamage()
        {
            if (target.enabled && target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(damage);
        }

#endregion

        private void Dead()
        {
            _thisCollider.enabled = false;
            navMeshAgent.enabled = false;
            anim.enabled = false;
            _deadSequence.Restart();
        }

#endregion

        protected virtual void DisableObject()
        {
            navMeshAgent.baseOffset = 0;
            navMeshAgent.enabled = false;
            _thisCollider.enabled = false;
            OnDisableEvent?.Invoke();
            OnDisableEvent = null;
            gameObject.SetActive(false);
        }

        public void SetSpeed(float animSpeed, float atkDelay)
        {
            navMeshAgent.speed = animSpeed;
            anim.speed = animSpeed;
            attackCooldown.cooldownTime = atkDelay;
        }

        public void DistanceToBaseTower()
        {
            var pos = transform.position;
            pos.y = 0;

            if (Vector3.Distance(pos, Vector3.zero) <= navMeshAgent.stoppingDistance)
            {
                if (gameObject.activeSelf)
                {
                    DisableObject();
                }
            }
        }
    }
}