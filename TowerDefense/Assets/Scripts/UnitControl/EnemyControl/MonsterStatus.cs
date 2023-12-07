using System;
using Cysharp.Threading.Tasks;
using StatusControl;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class MonsterStatus : MonoBehaviour
    {
        private MonsterUnit _monsterUnit;
        private NavMeshAgent _navMeshAgent;
        private bool _isSlowed;
        private float _defaultSpeed;

        private void Awake()
        {
            _monsterUnit = GetComponent<MonsterUnit>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void StatInit(float defaultSpeed)
        {
            _isSlowed = false;
            _defaultSpeed = defaultSpeed;
        }

        public void SlowEffect(byte decreaseSpeed, byte slowCoolTime)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            if (_navMeshAgent.speed - decreaseSpeed < 0.5f) _navMeshAgent.speed = 0.5f;
            else _navMeshAgent.speed -= decreaseSpeed;
            _monsterUnit.SetAnimationSpeed(_navMeshAgent.speed);
            SlowAsync(slowCoolTime).Forget();
        }

        private async UniTaskVoid SlowAsync(byte slowCoolTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            _navMeshAgent.speed = _defaultSpeed;
            _monsterUnit.SetAnimationSpeed(_defaultSpeed);
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime));
            _isSlowed = false;
        }
    }
}