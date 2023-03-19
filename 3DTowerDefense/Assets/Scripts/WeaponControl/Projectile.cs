using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private CancellationTokenSource _cts;
        private float _lerp;
        private Vector3 _curPos;

        public int damage;

        [SerializeField] private float lifeTime;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            DOVirtual.DelayedCall(lifeTime, DestroyProjectile);
            _lerp = 0;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Health h))
            {
                h.GetHit(damage, other.gameObject).Forget();
            }

            DestroyProjectile();
        }

        public async UniTaskVoid Parabola(Transform startPos, Vector3 endPos)
        {
            _lerp = 0;
            while (_lerp < 1)
            {
                var gravity = _lerp < 0.5f ? 1 : 1.5f;
                _lerp += Time.deltaTime * gravity * speed;

                _curPos = Vector3.Lerp(startPos.position, endPos, _lerp);
                _curPos.y += curve.Evaluate(_lerp);
                var dir = (_curPos - _rigid.position).normalized;
                _rigid.position = _curPos;
                _rigid.transform.forward = dir;
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
        }

        private void DestroyProjectile() => gameObject.SetActive(false);
    }
}