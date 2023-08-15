using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private float _lerp;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private AnimationCurve ProjectileCurve => curve;

        protected Transform target;
        protected Vector3 targetEndPos;
        private SphereCollider sphereCollider;
        private MeshRenderer _meshRenderer;
        private ParticleSystem _particle;
        private int _damage;

        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _particle = transform.GetChild(1).GetComponent<ParticleSystem>();
        }

        protected virtual void OnEnable()
        {
            _lerp = 0;
            _startPos = transform.position;
            sphereCollider.enabled = true;
            _meshRenderer.enabled = true;
        }

        protected abstract void FixedUpdate();

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                sphereCollider.enabled = false;
                ProjectileHit(other);
            }

            _rigid.velocity = Vector3.zero;
            _meshRenderer.enabled = false;
            _particle.Stop();
        }

        protected void ParabolaPath(Vector3 endPos)
        {
            var gravity = _lerp < 0.5f ? 1f : 1.2f;
            _lerp += Time.fixedDeltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, _lerp);
            _curPos.y += ProjectileCurve.Evaluate(_lerp);
            var t = _rigid.transform;
            var dir = (_curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = _curPos;
            t.forward = dir;
        }

        protected abstract void ProjectileHit(Collider col);

        protected void Damaging(Collider col)
        {
            if (col.TryGetComponent(out EnemyHealth h))
            {
                h.TakeDamage(_damage);
            }
        }

        public void Init(Transform t, int dmg)
        {
            target = t;
            _damage = dmg;
        }

        public void Init(Vector3 t, int dmg)
        {
            targetEndPos = t;
            _damage = dmg;
        }
    }
}