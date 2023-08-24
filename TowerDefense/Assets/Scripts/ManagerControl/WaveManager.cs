using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using StatusControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ManagerControl
{
    [Serializable]
    public struct Wave
    {
        public int startSpawnWave;
        public string enemyName;
        public int enemyCoin;
        public int atkRange;
        public float atkDelay;
        public int damage;
        public float health;
    }

    public class WaveManager : Singleton<WaveManager>
    {
        private bool _startGame;
        private int _curWave;
        private int _remainingEnemyCount;
        private CancellationTokenSource _cts;

        public event Action OnPlaceExpandButton;

        [SerializeField] private Wave[] waves;

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _curWave = 0;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        public void StartWave(Vector3[] wayPoints)
        {
            if (_startGame) return;
            _startGame = true;
            _curWave++;

            var wayPointsArray = wayPoints.ToArray();

            WaveInit(wayPointsArray);
            SpawnEnemy(wayPointsArray).Forget();
        }

        private void WaveInit(Vector3[] wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Length; i++)
            {
                for (var j = 0; j < waves.Length; j++)
                {
                    if (waves[j].startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }
        }

        private async UniTaskVoid SpawnEnemy(Vector3[] wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Length; i++)
            {
                if (i >= wayPointsArray.Length)
                {
                    i = 0;
                }

                for (var j = 0; j < waves.Length; j++)
                {
                    await UniTask.Delay(200, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray, i, j);
                }
            }
        }

        private void EnemyInit(IReadOnlyList<Vector3> wayPointsArray, int i, int j)
        {
            if (waves[j].startSpawnWave > _curWave) return;

            var waveIndex = j;
            var wave = waves[j];

            var enemyUnit = ObjectPoolManager.Get<EnemyUnit>(waves[j].enemyName, wayPointsArray[i]);
            enemyUnit.Init(wave);

            var disableEnemy = enemyUnit.GetComponent<DisableEnemyHandler>();
            disableEnemy.OnDecreaseLifeCount += TowerManager.Instance.DecreaseLifeCount;
            disableEnemy.OnUpdateEnemyCount += UpdateEnemyCount;

            var enemyHealth = enemyUnit.GetComponent<Health>();
            enemyHealth.Init(wave.health);
            enemyHealth.OnDie += () => TowerManager.Instance.IncreaseGold(waves[waveIndex].enemyCoin);
        }

        private void UpdateEnemyCount()
        {
            if (!_startGame) return;
            _remainingEnemyCount--;
            if (_remainingEnemyCount > 0) return;
            _startGame = false;
            OnPlaceExpandButton?.Invoke();
        }

        // [ContextMenu("To Json Data")]
        // private void SaveWaveDateToJson()
        // {
        //     DataManager.SaveDataToJson<WaveData>("waveData.json");
        // }
        //
        // [ContextMenu("From Json Data")]
        // private void LoadWaveDataToJson()
        // {
        //     waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        // }
    }
}