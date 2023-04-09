using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private float _lerp;
        private Vector3 _startPos, _endPos, _curPos;
        private int _damage;

        protected string TagName => tagName;

        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void OnEnable()
        {
            _lerp = 0;
            _startPos = transform.position;
        }

        private void FixedUpdate()
        {
            var gravity = _lerp < 0.5f ? 1 : 1.5f;
            _lerp += Time.deltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, _endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            t.position = _curPos;
            t.forward = dir;
        }

        protected abstract void OnTriggerEnter(Collider other);

        protected void GetDamage(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.GetHit(_damage, col.gameObject);
            }
        }

        public void Setting(Vector3 endPos, int damage)
        {
            _endPos = endPos;
            _damage = damage;
        }
    }
}