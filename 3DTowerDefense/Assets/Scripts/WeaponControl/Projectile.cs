using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private float _lerp;
        private Vector3 _startPos, _endPos, _curPos;

        private enum TargetName
        {
            Unit,
            Enemy,
            Ground
        }

        protected string tagName;

        public int damage;

        [SerializeField] private TargetName targetLayer;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            tagName = targetLayer.ToString();
        }

        protected virtual void OnEnable()
        {
            _lerp = 0;
            _startPos = transform.position;
        }

        private void FixedUpdate()
        {
            var gravity = _lerp < 0.5f ? 1 : 1.5f;
            _lerp += Time.fixedDeltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, _endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var dir = (_curPos - _rigid.position).normalized;
            _rigid.position = _curPos;
            _rigid.transform.forward = dir;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                gameObject.SetActive(false);
            }
        }

        protected void GetDamage(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.GetHit(damage, col.gameObject);
            }
        }

        public void SetPosition(Vector3 endPos)
        {
            _endPos = endPos;
        }
    }
}