using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private bool _isTargeting;
        private bool _isMoving;

        private Collider[] _targetColliders;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        protected Transform target;

        public event Action<Unit> OnDeadEvent;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] private float moveSpeed;

/*==============================================================================================================================================
                                                    Unity Event                                                                                 
==============================================================================================================================================*/

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[2];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void FixedUpdate()
        {
            if (_isMoving) return;
            if (!_isTargeting) return;
            if (Vector3.Distance(transform.position, target.position) > 2)
            {
                ChaseTarget();
            }
            else
            {
                DoAttack();
            }
        }

        private void LateUpdate()
        {
            Animation();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            TargetReset();
            OnDeadEvent?.Invoke(this);
            OnDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
/*==============================================================================================================================================
                                                    Unity Event                                                                                 
=====================================================================================================================================================*/

        private void ChaseTarget()
        {
            var targetPos = target.position + Random.insideUnitSphere * 2;
            var dir = (targetPos - rigid.position).normalized;
            rigid.velocity = dir * moveSpeed;
        }

        private void DoAttack()
        {
            if (!isCoolingDown) return;

            rigid.velocity = Vector3.zero;
            anim.SetTrigger(IsAttack);
            Attack();
            StartCoolDown().Forget();
            if (!target.gameObject.activeSelf)
            {
                _isTargeting = false;
            }
        }

        public async UniTaskVoid GoToTouchPosition(Vector3 pos)
        {
            _isMoving = true;
            isCoolingDown = false;
            await rigid.DOMove(pos, moveSpeed).SetSpeedBased().WithCancellation(cts.Token);
            _isMoving = false;
        }

        private void Animation()
        {
            anim.SetBool(IsWalk, _isMoving || _isTargeting);
        }

        private void Targeting()
        {
            if (_isMoving) return;
            if (target == null)
            {
                target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
                _isTargeting = target != null;
            }
            else
            {
                if (target.gameObject.activeSelf)
                {
                    var e = target.GetComponent<EnemyUnit>();
                    e.Target = transform;
                    e.IsTargeting = true;
                }
                else
                {
                    target = null;
                    _isTargeting = false;
                }
            }
        }

        private void TargetingPlease()
        {
            if (_isMoving) return;
            target = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            _isTargeting = target != null;

            if (_isTargeting)
            {
                if (target.gameObject.activeSelf)
                {
                    var e = target.GetComponent<EnemyUnit>();
                    e.Target = transform;
                    e.IsTargeting = true;
                }
            }
        }

        private void TargetReset()
        {
            if (!_isTargeting) return;
            var e = target.GetComponent<EnemyUnit>();
            e.Target = null;
            e.IsTargeting = false;
            target = null;
            _isTargeting = false;
        }
    }
}