using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using TowerControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyStatus : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private EnemyAI _enemyAI;
        private bool _isSlowed;
        private float _defaultSpeed;

        [SerializeField] private float slowImmunityTime;

        private void Awake()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _defaultSpeed = _enemyAI.MoveSpeed;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            StatusInit();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void StatusInit()
        {
            _isSlowed = false;
            _enemyAI.MoveSpeed = _defaultSpeed;
        }

        public async UniTaskVoid SlowEffect(DeBuffData.SpeedDeBuffData speedDeBuffData)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            _enemyAI.MoveSpeed -= speedDeBuffData.decreaseSpeed;
            if (_enemyAI.MoveSpeed < 0.5f) _enemyAI.MoveSpeed = 0.5f;
            await UniTask.Delay(TimeSpan.FromSeconds(speedDeBuffData.deBuffTime), cancellationToken: _cts.Token);
            _enemyAI.MoveSpeed += speedDeBuffData.decreaseSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(slowImmunityTime), cancellationToken: _cts.Token);
            _isSlowed = false;
        }
    }
}