using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterControl
{
    public class MonsterStatus : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private MonsterUnit _monsterUnit;
        private NavMeshAgent _navMeshAgent;
        private bool _isSlowed;
        private float _defaultSpeed;
        private float _defaultAtkDelay;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            _monsterUnit = GetComponent<MonsterUnit>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void StatInit(float defaultSpeed, float defaultAtkDelay)
        {
            _isSlowed = false;
            _defaultSpeed = defaultSpeed;
            _defaultAtkDelay = defaultAtkDelay;
        }

        public void SlowEffect(byte decreaseSpeed, byte slowCoolTime)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            var speed = _navMeshAgent.speed;

            if (speed - decreaseSpeed < 0.5f) speed = 0.5f;
            else speed -= decreaseSpeed;
            var increaseAtkDelay = _defaultAtkDelay + decreaseSpeed;
            _monsterUnit.SetSpeed(speed, increaseAtkDelay);
            SlowAsync(slowCoolTime).Forget();
        }

        private async UniTaskVoid SlowAsync(byte slowCoolTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            _navMeshAgent.speed = _defaultSpeed;
            _monsterUnit.SetSpeed(_defaultSpeed, _defaultAtkDelay);
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            _isSlowed = false;
        }
    }
}