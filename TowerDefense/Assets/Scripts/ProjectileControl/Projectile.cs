using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private float _lerp;
         
        protected int damage;
        protected string towerName;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            _startPos = transform.position;
        }

        protected abstract void FixedUpdate();

        protected virtual void OnDisable()
        {
            _lerp = 0;
            TryHit();
            ObjectPoolManager.ReturnToPool(gameObject);
        }

        protected void ParabolaPath(Vector3 endPos)
        {
            var gravity = _lerp < 0.5f ? 1f : 1.5f;
            _lerp += Time.deltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var rigidPos = _rigid.position;
            var dir = (_curPos - rigidPos).normalized;
            _rigid.position = _curPos;
            _rigid.MoveRotation(Quaternion.LookRotation(dir));

            if (_lerp >= 1)
            {
                gameObject.SetActive(false);
            }
        }

        protected abstract void TryHit();
    }
}