using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterControl
{
    public class MonsterStatus : MonoBehaviour
    {
        private MonsterUnit _monsterUnit;
        private bool _isSlowed;
        private float _defaultSpeed;
        private float _defaultAtkDelay;

        private void Awake()
        {
            _monsterUnit = GetComponent<MonsterUnit>();
        }

        public void StatInit(float defaultSpeed, float defaultAtkDelay)
        {
            _isSlowed = false;
            _defaultSpeed = defaultSpeed;
            _defaultAtkDelay = defaultAtkDelay;
        }

        public void SlowEffect(byte slowCoolTime)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            var speed = _monsterUnit.GetNavMeshSpeed();

            if (speed - slowCoolTime< 0.5f) speed = 0.5f;
            else speed -= slowCoolTime;
            var increaseAtkDelay = _defaultAtkDelay + slowCoolTime;
            _monsterUnit.SetSpeed(speed, increaseAtkDelay);
            SlowAsync(slowCoolTime).Forget();
        }

        private async UniTaskVoid SlowAsync(byte slowCoolTime)
        {
            var cts = this.GetCancellationTokenOnDestroy();
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime), cancellationToken: cts);
            _monsterUnit.SetSpeed(_defaultSpeed, _defaultAtkDelay);
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime), cancellationToken: cts);
            _isSlowed = false;
        }
    }
}