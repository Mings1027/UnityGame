using AttackControl;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private Collider[] _targetColliders;
        private Tween _delayTween;
        private int _minDamage, _maxDamage;

        protected bool attackAble;
        protected bool isTargeting;
        protected NavMeshAgent nav;
        protected Transform target;
        protected int Damage => Random.Range(_minDamage, _maxDamage);

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] [Range(0, 1)] private float smoothTurnSpeed;

        protected abstract void Attack();

        protected virtual void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
            _targetColliders = new Collider[5];
        }

        protected virtual void OnEnable()
        {
            attackAble = true;
            InvokeRepeating(nameof(FindTarget), 1f, 1f);
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;
            LookTarget();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public void UnitInit(int minDamage, int maxDamage, float delay)
        {
            _minDamage = minDamage;
            _maxDamage = maxDamage;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => attackAble = true).SetAutoKill(false);
        }

        private void FindTarget()
        {
            var c = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            target = c.Item1;
            isTargeting = c.Item2;
        }

        protected void StartCoolDown()
        {
            attackAble = false;
            _delayTween.Restart();
        }

        private void LookTarget()
        {
            var direction = target.position + target.forward;
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var lookRot = Quaternion.Euler(0, yRot, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothTurnSpeed);
        }
    }
}