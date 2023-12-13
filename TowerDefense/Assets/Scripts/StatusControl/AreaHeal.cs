using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StatusControl
{
    public class AreaHeal : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private Collider[] _targetColliders;
        private LayerMask _targetLayer;

        [SerializeField] private byte healTargetCount;
        [SerializeField] private byte healAmount;
        [SerializeField] private byte healRange;
        [SerializeField] private byte healCooldown;

        private void Awake()
        {
            _targetColliders = new Collider[healTargetCount];
            _targetLayer = LayerMask.GetMask("Monster") | LayerMask.GetMask("FlyingMonster");
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            Healing().Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healRange);
        }

        private async UniTaskVoid Healing()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(healCooldown), cancellationToken: _cts.Token);
                var size = Physics.OverlapSphereNonAlloc(transform.position, healRange, _targetColliders,
                    _targetLayer);
                for (var i = 0; i < size; i++)
                {
                    _targetColliders[i].GetComponent<Health>().Heal(healAmount);
                }
            }
        }
    }
}