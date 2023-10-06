using System;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using ManagerControl;
using Pathfinding;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Animator _anim;
        private Rigidbody _rigid;
        private SphereCollider _sphereCollider;
        private UnitAI _unitAI;
        private EnemyHealth _enemyHealth;
        private Collider[] _targetCollider;
        private AttackPoint _attackPoint;

        private Collider _target;
        private Cooldown atkCooldown;
        private bool _isTargeting;
        private bool _targetInAtkRange;

        private int _damage;

        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        private static readonly int IsDead = Animator.StringToHash("isDead");

        [SerializeField] private float atkRange;
        [SerializeField] private float sightRange;
        [SerializeField] private LayerMask targetLayer;

        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _anim = GetComponentInChildren<Animator>();
            _rigid = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
            _unitAI = GetComponent<UnitAI>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            _attackPoint = GetComponentInChildren<AttackPoint>();
        }

        private void OnEnable()
        {
            _target = null;
            _isTargeting = false;
            _enemyHealth.OnDeadEvent += DeadAnimation;
            SetAnimationSpeed(_unitAI.MoveSpeed);

            InvokeRepeating(nameof(Targeting), 0f, 1f);
        }

        private void FixedUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _targetInAtkRange = Physics.CheckSphere(transform.position, atkRange, targetLayer);

            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }

        private void Update()
        {
            if (_enemyHealth.IsDead) return;
            if (_targetInAtkRange && _isTargeting)
            {
                _unitAI.CanMove = false;
                if (atkCooldown.IsCoolingDown) return;
                Attack();
                atkCooldown.StartCooldown();
            }
        }

        private void LateUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, _unitAI.CanMove);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Destination"))
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
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        public void Targeting()
        {
            if (_enemyHealth.IsDead) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _unitAI.CanMove = true;
                _unitAI.targetPos = Vector3.zero;

                _sphereCollider.isTrigger = true;
                return;
            }

            if (!_isTargeting)
            {
                _isTargeting = true;
                _target = _targetCollider[0];
                _sphereCollider.isTrigger = false;
            }

            if (_target) _unitAI.targetPos = _target.transform.position;
        }

        private void Attack()
        {
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            if (_target.enabled) return;
            _target = null;
            _isTargeting = false;
        }

        public void SetAnimationSpeed(float animSpeed) => _anim.speed = animSpeed * 0.5f;

        private void DeadAnimation()
        {
            _unitAI.CanMove = false;
            _anim.SetTrigger(IsDead);
            DOVirtual.DelayedCall(2, () => gameObject.SetActive(false));
        }

        public void Init(WaveData.EnemyInfo info)
        {
            atkCooldown.cooldownTime = info.atkDelay;
            _damage = info.damage;
        }
    }
}