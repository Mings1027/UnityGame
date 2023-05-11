using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private Tween _delayTween;
        private int _minDamage, _maxDamage;

        protected GameManager gameManager;
        protected bool attackAble;
        protected NavMeshAgent nav;

        protected int Damage => Random.Range(_minDamage, _maxDamage);
        protected int AtkRange => atkRange;
        protected LayerMask TargetLayer => targetLayer;

        public Transform Target { get; set; }
        public bool IsTargeting { get; set; }

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int atkRange;
        [SerializeField] [Range(0, 1)] private float smoothTurnSpeed;

        protected abstract void Attack();

        protected virtual void Awake()
        {
            nav = GetComponent<NavMeshAgent>();

            gameManager = GameManager.Instance;
        }

        protected virtual void OnEnable()
        {
            attackAble = true;
        }

        protected abstract void Update();

        private void LateUpdate()
        {
            if (gameManager.IsPause) return;
            if (!IsTargeting) return;
            LookTarget();
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public void Init(int minDamage, int maxDamage, float delay)
        {
            _minDamage = minDamage;
            _maxDamage = maxDamage;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => attackAble = true, false).SetAutoKill(false);
        }

        protected void StartCoolDown()
        {
            attackAble = false;
            _delayTween.Restart();
        }

        private void LookTarget()
        {
            var direction = Target.position +Target.forward;
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var lookRot = Quaternion.Euler(0, yRot, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothTurnSpeed);
        }
    }
}