using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using PoolObjectControl;
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
            _enemyAI = GetComponent<EnemyAI>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _targetCollider = new Collider[1];
            _t = transform;
            _attackPoint = GetComponentInChildren<AttackPoint>();
            _targetingTime.cooldownTime = 2f;
        }

        private void OnEnable()
        {
            _target = null;
            _isTargeting = false;
            _isAttack = false;
            _enemyHealth.OnDieEvent += DeadAnimation;
        }

        private void FixedUpdate()
        {
            if (_enemyHealth.IsDead) return;
            _targetInAtkRange = Physics.CheckSphere(transform.position, _sphereCollider.radius, targetLayer);

            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }

        private void Update()
        {
            if (_enemyHealth.IsDead) return;
            Targeting();

            if (_isTargeting)
            {
                if (_targetInAtkRange)
                {
                    Attack();
                }
                else
                {
                    Chase();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Destination"))
                gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _enemyHealth.OnDieEvent -= DeadAnimation;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            if (_sphereCollider == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
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
                _sphereCollider.isTrigger = true;
                _anim.SetBool(IsWalk, true);
                return;
            }

            _target = _targetCollider[0].transform;
            _isTargeting = true;
            _enemyAI.CanMove = false;
            _sphereCollider.isTrigger = false;
            _targetingTime.StartCooldown();
        }

        private void Chase()
        {
            _anim.SetBool(IsWalk, true);
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

        private void DeadAnimation()
        {
            _anim.SetBool(IsWalk, false);
            _enemyAI.CanMove = false;
            var pos = transform.position;
            transform.DORotate(new Vector3(-90, pos.y, pos.z), 0.5f, RotateMode.LocalAxisAdd).OnComplete(() =>
            {
                transform.DOScale(0, 1).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                });
            });
        }

        public void Init(WaveData.EnemyInfo info)
        {
            _atkCooldown.cooldownTime = info.atkDelay;
            _damage = info.damage;
        }
    }
}