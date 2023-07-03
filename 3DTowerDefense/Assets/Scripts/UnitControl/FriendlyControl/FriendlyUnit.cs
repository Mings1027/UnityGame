using System;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private bool _isTargeting;
        private Vector3 _touchPos;

        protected Transform target;
        private bool _isMoving;

        private Collider[] _targetColliders;

        public event Action<Unit> OnDeadEvent;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] [Range(0, 1)] private float turnSpeed;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[2];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(Targeting), 1, 0.5f);
        }

        private void FixedUpdate()
        {
            if (_isMoving)
            {
                if (Vector3.Distance(transform.position, _touchPos) <= nav.stoppingDistance)
                {
                    _isMoving = false;
                }
            }
            else
            {
                if (!_isTargeting) return;

                if (attackAble)
                {
                    if (Vector3.Distance(transform.position, target.position) <= nav.stoppingDistance)
                    {
                        Attack();
                        StartCoolDown().Forget();
                    }
                }

                nav.SetDestination(target.position);
            }
        }

        private void LateUpdate()
        {
            if (!_isTargeting) return;
            LookTarget();
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

        public void GoToTouchPosition(Vector3 pos)
        {
            _isMoving = true;
            _touchPos = pos;
            nav.SetDestination(_touchPos);
        }

        private void LookTarget()
        {
            var dir = (target.position - transform.position).normalized;
            var lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed);
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

        private void TargetReset()
        {
            if (!_isTargeting) return;
            var e = target.GetComponent<EnemyUnit>();
            e.Target = null;
            e.IsTargeting = false;
            target = null;
            _isTargeting = false;
        }

        // private (Transform, bool) SearchTargetPlease()
        // {
        //     var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, targetColliders, targetLayer);
        //     if (size <= 0) return (null, false);
        //
        //     var shortestDistance = Mathf.Infinity;
        //     Transform nearestTarget = null;
        //
        //     for (var i = 0; i < size; i++)
        //     {
        //         var disToTarget = Vector3.SqrMagnitude(transform.position - targetColliders[i].transform.position);
        //         if (disToTarget >= shortestDistance) continue;
        //         shortestDistance = disToTarget;
        //         nearestTarget = targetColliders[i].transform;
        //     }
        //
        //     return (nearestTarget, true);
        // }
    }
}