using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.EnemyControl
{
    public class EnemyStatus : MonoBehaviour
    {
        private EnemyUnit _enemyUnit;
        private NavMeshAgent _navMeshAgent;
        private EnemyHealth _enemyHealth;
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
            SlowEffectTween(slowCoolTime).Forget();
        }

        private async UniTaskVoid SlowEffectTween(byte slowCoolTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime));
            _navMeshAgent.speed = defaultSpeed;
            _enemyUnit.SetAnimationSpeed(defaultSpeed);
            await UniTask.Delay(TimeSpan.FromSeconds(slowCoolTime));
            _isSlowed = false;
        }

        public void ContinuousDamage()
        {
            _enemyHealth.Damage(1);
        }
    }
}