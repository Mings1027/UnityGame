using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TowerControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyStatus : MonoBehaviour
    {
        private CancellationTokenSource cts;
        private EnemyAI enemyAI;
        private bool isSlowed;
        private float defaultSpeed;

        private void Awake()
        {
            enemyAI = GetComponent<EnemyAI>();
            defaultSpeed = enemyAI.MoveSpeed;
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            StatusInit();
        }

        private void OnDisable()
        {
            cts?.Cancel();
        }

        private void StatusInit()
        {
            isSlowed = false;
            enemyAI.MoveSpeed = defaultSpeed;
        }

        public async UniTaskVoid SlowEffect(SpeedDeBuffData speedDeBuffData)
        {
            if (isSlowed) return;
            isSlowed = true;
            enemyAI.MoveSpeed -= speedDeBuffData.decreaseSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(speedDeBuffData.deBuffTime), cancellationToken: cts.Token);
            enemyAI.MoveSpeed += speedDeBuffData.decreaseSpeed;
            isSlowed = false;
        }
    }
}