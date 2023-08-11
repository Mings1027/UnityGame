using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ManagerControl
{
    public class WaveManager : Singleton<WaveManager>
    {
        private bool _startGame;
        private int _curWave;
        private int _remainingEnemyCount;
        private CancellationTokenSource _cts;

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

        public event Action OnPlaceExpandBtnEvent;

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

        public void StartWave(HashSet<Vector3> wayPoints)
        {
            SpawnEnemyTestTest(wayPoints).Forget();
        }

        private async UniTaskVoid SpawnEnemyTestTest(HashSet<Vector3> wayPoints)
        {
            if (_startGame) return;
            _startGame = true;
            _curWave++;
            var wayPointsArray = wayPoints.ToArray();

            for (var i = 0; i < wayPoints.Count; i++)
            {
                for (var j = 0; j < waves.Length; j++)
                {
                    if (waves[j].startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }

            for (var i = 0; i < wayPoints.Count; i++)
            {
                if (i >= wayPointsArray.Length)
                {
                    i = 0;
                }

                for (var j = 0; j < waves.Length; j++)
                {
                    await UniTask.Delay(200, cancellationToken: _cts.Token);
                    if (waves[j].startSpawnWave <= _curWave)
                    {
                        var enemyUnit = ObjectPoolManager.Get<EnemyUnit>(waves[j].enemyName, wayPointsArray[i]);
                        var enemyHealth = enemyUnit.GetComponent<Health>();
                        enemyHealth.OnDeadEvent += DeadEnemy;

                        var waveIndex = j;
                        enemyHealth.OnIncreaseCoinEvent += () =>
                            TowerManager.Instance.IncreaseCoin(waves[waveIndex].enemyCoin);
                        enemyHealth.OnDecreaseLifeCountEvent +=
                            TowerManager.Instance.DecreaseLifeCount;

                        var wave = waves[j];
                        enemyUnit.Init(wave.atkRange, wave.atkDelay, wave.damage, wave.health);
                    }
                }
            }
        }

        private void DeadEnemy()
        {
            _remainingEnemyCount--;
            if (_remainingEnemyCount > 0) return;
            _startGame = false;
            OnPlaceExpandBtnEvent?.Invoke();
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