using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private float _lerp;
        private Vector3 _startPos, _endPos, _curPos;

        public int damage;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _lerp = 0;
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
            if (other.TryGetComponent(out Health h))
            {
                h.GetHit(damage, other.gameObject);
            }

            if (other.CompareTag("Ground"))
                gameObject.SetActive(false);
        }

        public void SetPosition(Vector3 startPos, Vector3 endPos)
        {
            _startPos = startPos;
            _endPos = endPos;
        }
    }
}