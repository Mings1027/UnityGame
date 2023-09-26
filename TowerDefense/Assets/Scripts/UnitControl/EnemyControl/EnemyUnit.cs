using DataControl;
using DG.Tweening;
using ManagerControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Animator _anim;
        private Rigidbody _rigid;

        private BoxCollider _boxCollider;
        private EnemyAI _enemyAI;
        private EnemyHealth _enemyHealth;
        private Collider[] _targetCollider;
        private AttackPoint _attackPoint;

        private Transform _target;
        private Transform _t;

        private bool _isAttack;
        private bool _isTargeting;
        private bool _targetInAtkRange;

        private int _damage;

        private Cooldown _atkCooldown;
        private Cooldown _targetingTime;

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
            _boxCollider = GetComponent<BoxCollider>();
            _enemyAI = GetComponent<EnemyAI>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            _t = transform;
            _attackPoint = GetComponentInChildren<AttackPoint>();
            _targetingTime.cooldownTime = 2f;
            atkRange = _boxCollider.size.z;
        }

        private void OnEnable()
        {
            _target = null;
            _isTargeting = false;
            _isAttack = false;
            _enemyHealth.OnDeadEvent += DeadAnimation;
            SetAnimationSpeed(_enemyAI.MoveSpeed);
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
            Targeting();

            if (!_isTargeting) return;

            if (_targetInAtkRange)
            {
                Attack();
            }
            else
            {
                Chase();
            }
        }

        private void LateUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _anim.SetBool(IsWalk, !_targetInAtkRange);
            // _anim.speed = _targetInAtkRange ? 1 : _enemyAI.MoveSpeed * 0.5f;
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
            if (_boxCollider == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
#endif
        /*==============================================================================================================================================
                                                    Unity Event
=====================================================================================================================================================*/

        private void Targeting()
        {
            if (_targetingTime.IsCoolingDown) return;
            if (_enemyHealth.IsDead) return;
            if (_isAttack) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetCollider, targetLayer);
            if (size <= 0)
            {
                _target = null;
                _isTargeting = false;
                _enemyAI.CanMove = true;
                _boxCollider.isTrigger = true;
                return;
            }

            _target = _targetCollider[0].transform;
            _isTargeting = true;
            _enemyAI.CanMove = false;
            _boxCollider.isTrigger = false;
            _targetingTime.StartCooldown();
        }

        private void Chase()
        {
            var targetPos = _target.position;
            var position = _rigid.position;
            var dir = (targetPos - position).normalized;
            var moveVec = dir * (_enemyAI.MoveSpeed * Time.deltaTime);
            _rigid.MovePosition(position + moveVec);
            _t.forward = (targetPos - position).normalized;
        }

        private void Attack()
        {
            if (_atkCooldown.IsCoolingDown) return;
            if (_attackPoint.enabled) return;
            _anim.SetTrigger(IsAttack);
            _audioSource.Play();
            _attackPoint.Init(_target, _damage);
            _attackPoint.enabled = true;
            _atkCooldown.StartCooldown();
            if (_target.gameObject.activeSelf) return;
            _target = null;
            _isTargeting = false;
        }

        public void SetAnimationSpeed(float animSpeed)
        {
            _anim.speed = animSpeed * 0.5f;
        }

        private void DeadAnimation()
        {
            _enemyAI.CanMove = false;
            _anim.SetTrigger(IsDead);
            DOVirtual.DelayedCall(2, () => gameObject.SetActive(false));
        }

        public void Init(WaveData.EnemyInfo info)
        {
            _atkCooldown.cooldownTime = info.atkDelay;
            _damage = info.damage;
        }
    }
}