using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Projectile : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private Rigidbody _rigid;
        private float _lerp;
        private Vector3 _curPos;

        public Vector3 startPos;
        public Transform target;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;
        [SerializeField] private float lifeTime;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            startPos = Vector3.zero;
            target = transform;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Invoke(nameof(DestroyProjectile), lifeTime);
            Parabola().Forget();
        }

        private void OnDisable()
        {
            _cts.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            _lerp = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Ground")) DestroyProjectile();
        }

        private async UniTaskVoid Parabola()
        {
            while (_lerp < 1)
            {
                _lerp += Time.deltaTime * speed;
                _curPos = Vector3.Lerp(startPos, target.position, _lerp);
                _curPos.y += curve.Evaluate(_lerp);
                print(Time.deltaTime);
                transform.position = _curPos;
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
        }

        private void DestroyProjectile() => gameObject.SetActive(false);
    }
}