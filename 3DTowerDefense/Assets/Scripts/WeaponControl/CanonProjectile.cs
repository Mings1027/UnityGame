using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class CanonProjectile : Projectile
    {
        private Vector3 _targetPos;
        private Collider[] _targetColliders;

        public MeshFilter CanonMeshFilter { get; private set; }

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            CanonMeshFilter = GetComponentInChildren<MeshFilter>();
            _targetColliders = new Collider[5];
        }

        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Ground")) return;
            ProjectileHit(other);
            gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ParabolaPath()
        {
            var gravity = lerp < 0.5f ? 1f : 1.2f;
            lerp += Time.fixedDeltaTime * gravity * ProjectileSpeed;
            curPos = Vector3.Lerp(startPos, _targetPos, lerp);
            curPos.y += ProjectileCurve.Evaluate(lerp);
            var t = rigid.transform;
            var dir = (curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = curPos;
            t.forward = dir;
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.CanonHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.CanonHitSfx, pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                Damaging(_targetColliders[i]);
            }
        }

        public void Init(Vector3 pos, int dmg)
        {
            _targetPos = pos;
            damage = dmg;
        }
    }
}