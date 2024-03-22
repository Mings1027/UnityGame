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
using Random = UnityEngine.Random;

namespace MonsterControl
{
    public abstract class MonsterUnit : MonoBehaviour
    {
        private Transform _childMeshTransform;
        private Sequence _deadSequence;
        private Collider _thisCollider;
        private Cooldown _patrolCooldown;

        private LayerMask _targetLayer;
        private Health _health;
        private readonly int _isWalk = Animator.StringToHash("isWalk");

        protected NavMeshAgent navMeshAgent;
        protected Animator anim;
        protected Collider target;
        protected Collider[] targetCollider;
        protected UnitState unitState;
        protected Cooldown attackCooldown;
        protected readonly int isAttack = Animator.StringToHash("isAttack");

        public Transform healthBarTransform { get; private set; }
        public event Action OnDisableEvent;

        [SerializeField] protected MonsterData monsterData;

#region Unity Event

        protected virtual void Awake()
        {
            _childMeshTransform = transform.GetChild(0);
            healthBarTransform = transform.GetChild(1);
            _targetLayer = LayerMask.GetMask("Unit");
            targetCollider = new Collider[monsterData.maxDetectedCount];

            anim = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            _thisCollider = GetComponent<Collider>();
            GetComponent<Rigidbody>();
            _health = GetComponent<Health>();

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
            var position = transform.position;
            var checkPos = new Vector3(position.x, 0, position.z);

            Gizmos.DrawWireSphere(checkPos, monsterData.attackRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(checkPos, monsterData.sightRange);
        }

#endregion

#region Init

        public virtual void Init()
        {
            _thisCollider.enabled = true;
            target = null;
            _patrolCooldown.cooldownTime = 0.5f;

            _health.OnDeadEvent += Dead;
            OnDisableEvent += () =>
            {
                if (!_health.isDead) GameHUD.towerHealth.Damage(monsterData.baseTowerDamage);
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
            anim.SetBool(_isWalk, true);
        }

#endregion

#region Unit Update

        public virtual void MonsterUpdate()
        {
            if (!navMeshAgent.enabled) return;
            if (_health.isDead) return;
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

            anim.SetBool(_isWalk, navMeshAgent.velocity != Vector3.zero);

            var dir = navMeshAgent.desiredVelocity;
            if (dir == Vector3.zero) return;
            var rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10);
        }

        public void DistanceToBaseTower()
        {
            if (unitState == UnitState.Attack) return;
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
            if (_patrolCooldown.IsCoolingDown) return;
            var position = transform.position;
            var checkPos = new Vector3(position.x, 0, position.z);
            var size = Physics.OverlapSphereNonAlloc(checkPos, monsterData.sightRange, targetCollider,
                _targetLayer);
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

            _patrolCooldown.StartCooldown();
            unitState = UnitState.Chase;
        }

        protected virtual void Chase()
        {
            if (!target.enabled)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(target.transform.position);
            }

            var position = transform.position;
            var checkPos = new Vector3(position.x, 0, position.z);

            if (Vector3.Distance(target.transform.position, checkPos) <= monsterData.attackRange)
            {
                unitState = UnitState.Attack;
            }
        }

        protected virtual void Attack()
        {
            var position = transform.position;
            var checkPos = new Vector3(position.x, 0, position.z);

            if (!target || !target.enabled ||
                Vector3.Distance(target.transform.position, checkPos) > monsterData.attackRange)
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

        public float GetNavMeshSpeed() => navMeshAgent.speed;
    }
}