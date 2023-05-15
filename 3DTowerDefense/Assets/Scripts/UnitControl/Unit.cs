using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private Tween _delayTween;
        private int _minDamage, _maxDamage;

        protected Collider[] targetColliders;
        protected bool attackAble;
        protected NavMeshAgent nav;
        protected Transform target;

        protected int Damage => Random.Range(_minDamage, _maxDamage);

        public bool IsTargeting { get; protected set; }

        [SerializeField] [Range(0, 1)] private float smoothTurnSpeed;

        protected virtual void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
        }

        protected virtual void OnEnable()
        {
            IsTargeting = false;
            attackAble = true;
        }

        protected abstract void Update();

        private void LateUpdate()
        {
            if (!IsTargeting) return;
            LookTarget();
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        protected abstract void Attack();

        protected void StartCoolDown()
        {
            attackAble = false;
            _delayTween.Restart();
        }

        public void Init(int minD, int maxD, float delay)
        {
            _minDamage = minD;
            _maxDamage = maxD;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => attackAble = true, false).SetAutoKill(false);
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