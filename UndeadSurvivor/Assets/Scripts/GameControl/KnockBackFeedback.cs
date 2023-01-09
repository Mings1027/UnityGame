using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GameControl
{
    public class KnockBackFeedback : MonoBehaviour
    {
        private Rigidbody2D _rigid;
        [SerializeField] private float strength, delay;

        [SerializeField] private UnityEvent onBegin, onDone;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        public void KnockBack(GameObject sender)
        {
            StartKnockBack(sender).Forget();
        }

        private async UniTaskVoid StartKnockBack(GameObject sender)
        {
            onBegin?.Invoke();
            var dir = (_rigid.position - (Vector2)sender.transform.position).normalized;
            _rigid.AddForce(dir * strength, ForceMode2D.Impulse);
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _cts.Token);
            _rigid.velocity = Vector2.zero;
            onDone?.Invoke();
        }
    }
}