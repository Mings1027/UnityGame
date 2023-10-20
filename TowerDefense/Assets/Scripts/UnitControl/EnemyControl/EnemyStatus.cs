using System;
using Cysharp.Threading.Tasks;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyStatus : MonoBehaviour
    {
        private EnemyUnit _enemyUnit;
        private NavMeshAgent _navMeshAgent;
        private bool _isSlowed;
        public float defaultSpeed { get; set; }

        private void Awake()
        {
            _enemyUnit = GetComponent<EnemyUnit>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _isSlowed = false;
        }

        public void SlowEffect(byte decreaseSpeed, byte slowCoolTime)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            if (_navMeshAgent.speed - decreaseSpeed < 0.5f) _navMeshAgent.speed = 0.5f;
            else _navMeshAgent.speed -= decreaseSpeed;
            _enemyUnit.SetAnimationSpeed(_navMeshAgent.speed);
            SlowAsync(slowCoolTime).Forget();
        }

        private async UniTaskVoid SlowAsync(byte slowCoolTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            _navMeshAgent.speed = defaultSpeed;
            _enemyUnit.SetAnimationSpeed(defaultSpeed);
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            _isSlowed = false;
        }
    }
}