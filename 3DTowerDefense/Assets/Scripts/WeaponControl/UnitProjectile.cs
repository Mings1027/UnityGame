using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class UnitProjectile : MonoBehaviour
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
            Invoke(nameof(DestroyProjectile), lifeTime);
            _lerp = 0;
        }
        
        private void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit")) return;
            if (other.CompareTag("Enemy") || other.CompareTag("Ground")) DestroyProjectile();
            if (other.TryGetComponent(out Health h))
            {
                h.GetHit(damage,other.gameObject).Forget();
            }
        }

        public async UniTaskVoid Parabola(Transform startPos, Vector3 endPos)
        {
            _lerp = 0;
            while (_lerp < 1)
            {
                _lerp += Time.deltaTime * speed;
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