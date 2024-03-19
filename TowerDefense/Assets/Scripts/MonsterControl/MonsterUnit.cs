using System;
using System.Diagnostics;
using CustomEnumControl;
using DataControl.MonsterDataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using StatusControl;
using UIControl;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
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

        private readonly int _isWalk = Animator.StringToHash("isWalk");
        protected readonly int isAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField] protected MonsterData monsterData;

#region Unity Event

        protected virtual void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            targetLayer = LayerMask.GetMask("Unit");
            targetCollider = new Collider[monsterData.maxDetectedCount];
            
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
                    _childMeshTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    transform.localScale = Vector3.one;
                    DisableObject();
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
            Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, monsterData.sightRange);
        }

#endregion

#region Init

        public virtual void Init()
        {
            _thisCollider.enabled = true;
            target = null;
            patrolCooldown.cooldownTime = 0.5f;

            health.OnDeadEvent += Dead;
            OnDisableEvent += () =>
            {
                if (!health.isDead) GameHUD.towerHealth.Damage(monsterData.baseTowerDamage);
            };
            anim.enabled = true;
        }

        public virtual void SpawnInit()
        {
            navMeshAgent.enabled = true;
            unitState = UnitState.Patrol;
            navMeshAgent.speed = monsterData.speed;
            SetSpeed(navMeshAgent.speed, attackCooldown.cooldownTime);
            attackCooldown.cooldownTime = monsterData.attackDelay;
            if (navMeshAgent.isOnNavMesh) navMeshAgent.SetDestination(Vector3.zero);
            anim.SetBool(_isWalk, true);
        }

#endregion

#region Unit Update

        public virtual void MonsterUpdate()
        {
            if (!navMeshAgent.enabled) return;
            if (health.isDead) return;
            switch (unitState)
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

            if (target && target.enabled)
            {
                var dir = (target.transform.position - transform.position).normalized;
                var targetRot = Quaternion.LookRotation(dir);
                var eulerAngleDiff = targetRot.eulerAngles - transform.rotation.eulerAngles;
                transform.Rotate(eulerAngleDiff);
            }

            anim.SetBool(_isWalk, navMeshAgent.velocity != Vector3.zero);
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

#region Monster State

        protected virtual void Patrol()
        {
            if (patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, monsterData.sightRange, targetCollider,
                targetLayer);
            if (size <= 0)
            {
                target = null;
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(Vector3.zero);
                }

                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - targetCollider[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    target = targetCollider[i];
                }
            }

            unitState = UnitState.Chase;
            patrolCooldown.StartCooldown();
        }

        protected virtual void Chase()
        {
            if (!target.enabled)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.SetDestination(target.transform.position +
                                            Random.insideUnitSphere * monsterData.attackRange);
            if (Vector3.Distance(target.transform.position, transform.position) <= monsterData.attackRange)
            {
                unitState = UnitState.Attack;
            }
        }

        protected virtual void Attack()
        {
            if (!target || !target.enabled ||
                Vector3.Distance(target.transform.position, transform.position) > monsterData.attackRange)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (attackCooldown.IsCoolingDown) return;

            anim.SetTrigger(isAttack);
            TryDamage();

            attackCooldown.StartCooldown();
        }

        protected virtual void TryDamage()
        {
            if (target.enabled && target.TryGetComponent(out IDamageable damageable))
                damageable.Damage(monsterData.damage);
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

        private void DisableObject()
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
    }
}