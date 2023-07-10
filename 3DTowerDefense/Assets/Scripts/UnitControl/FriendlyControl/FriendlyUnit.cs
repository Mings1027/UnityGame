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
            target = null;
            _isTargeting = false;
            InvokeRepeating(nameof(Targeting), 1f, 1f);
        }

        private void Update()
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
            anim.SetBool(IsWalk, _isMoving || _isTargeting);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            OnDeadEvent?.Invoke(this);
            OnDeadEvent = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            if (!_isTargeting) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(target.position + new Vector3(0, 3, 0), 1);
        }
/*==============================================================================================================================================
                                                    Unity Event                                                                                 
=====================================================================================================================================================*/

        public void MoveToTouchPos(Vector3 pos)
        {
            var ranPos = pos + Random.insideUnitSphere * 3f;
            ranPos.y = 1;
            rigid.DOKill();
            rigid.DOMove(ranPos, moveSpeed).SetSpeedBased()
                .OnStart(() => _isMoving = true)
                .OnComplete(() => _isMoving = false);
        }

        private void ChaseTarget()
        {
            var targetPos = target.position + Random.insideUnitSphere * 2;
            var position = rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (moveSpeed * Time.deltaTime);
            rigid.MovePosition(position + moveVec);
        }

        private void DoAttack()
        {
            if (!isCoolingDown) return;

            if (!target.gameObject.activeSelf)
            {
                target = null;
                _isTargeting = false;
                return;
            }

            anim.SetTrigger(IsAttack);
            Attack();
            StartCoolDown().Forget();
        }

        private void Targeting()
        {
            if (_isMoving) return;
            var t = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            target = t.Item1;
            _isTargeting = t.Item2;

            if (!_isTargeting) return;
            var e = target.GetComponent<EnemyUnit>();
            e.Target = transform;
            e.IsTargeting = true;
        }
    }
}