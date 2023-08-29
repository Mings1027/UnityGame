using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using InterfaceControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private Animator _anim;
        private Rigidbody _rigid;
        private EnemyAI _enemyAI;
        private Collider[] _targetCollider;
        private CancellationTokenSource _cts;
        private Transform _target;
        private Transform _t;

        private bool _isAttack;
        private bool _isTargeting;
        private int _damage;
        private int _attackRange;
        private float _atkDelay;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        [SerializeField] private LayerMask targetLayer;
/*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _rigid = GetComponent<Rigidbody>();
            _enemyAI = GetComponent<EnemyAI>();
            _targetCollider = new Collider[3];
            _t = transform;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _target = null;
            _isTargeting = false;
            _isAttack = false;
            InvokeRepeating(nameof(Targeting), 0, 1);
        }

        private void FixedUpdate()
        {
            if (!_isTargeting) return;

            if (Vector3.Distance(_t.position, _target.position) > 1)
            {
                ChaseTarget();
            }
        }

        private void LateUpdate()
        {
            _anim.SetBool(IsWalk, _enemyAI.CanMove);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_t.position, _attackRange);
        }
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void ChaseTarget()
        {
            var targetPos = _target.position;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (_enemyAI.MoveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
            _t.forward = (targetPos - position).normalized;
        }

        private void DoAttack()
        {
            if (_isAttack) return;
            if (!_target.gameObject.activeSelf)
            {
                _target = null;
                _isTargeting = false;
                return;
            }

            _t.forward = (_target.position - _t.position).normalized;
            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            _isTargeting = false;
            _isAttack = true;
            _anim.SetTrigger(IsAttack);
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);

            _isAttack = false;
        }

        private void Targeting()
        {
            if (_isAttack) return;
            _target = SearchTarget.ClosestTarget(transform.position, _attackRange, _targetCollider, targetLayer);
            _isTargeting = _target != null;
            _enemyAI.CanMove = !_isTargeting;

            if (_isTargeting)
            {
                if (Vector3.Distance(_t.position, _target.position) <= 1)
                {
                    DoAttack();
                }
            }
        }

        public void Init(WaveData.EnemyWave wave)
        {
            _attackRange = wave.atkRange;
            _atkDelay = wave.atkDelay;
            _damage = wave.damage;
        }
    }
}