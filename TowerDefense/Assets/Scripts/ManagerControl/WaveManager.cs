using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
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

        public event Action OnPlaceExpandButton;

        [SerializeField] private WaveData waveData;

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

            TowerManager.Instance.WaveText.text = "Wave : " + _curWave;

            WaveInit(wayPoints);
            SpawnEnemy(wayPoints).Forget();
        }

        private void WaveInit(Vector3[] wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Length; i++)
            {
                for (var j = 0; j < waveData.enemyWaves.Length; j++)
                {
                    if (waveData.enemyWaves[j].startSpawnWave <= _curWave)
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

                for (var j = 0; j < waveData.enemyWaves.Length; j++)
                {
                    await UniTask.Delay(200, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray, i, j);
                }
            }
        }

        private void EnemyInit(IReadOnlyList<Vector3> wayPointsArray, int i, int j)
        {
            if (waveData.enemyWaves[j].startSpawnWave > _curWave) return;

            var waveIndex = j;
            var wave = waveData.enemyWaves[j];

            var enemyUnit = ObjectPoolManager.Get<EnemyUnit>(waveData.enemyWaves[j].enemyName, wayPointsArray[i]);
            enemyUnit.Init(wave);

            var enemyHealth = enemyUnit.GetComponent<EnemyHealth>();

            enemyHealth.Init(wave.health);
            enemyHealth.OnDecreaseLifeCountEvent += TowerManager.Instance.DecreaseLifeCountEvent;
            enemyHealth.OnUpdateEnemyCountEvent += UpdateEnemyCountEvent;
            enemyHealth.OnDieEvent +=
                () => TowerManager.Instance.IncreaseGold(waveData.enemyWaves[waveIndex].enemyCoin);
        }

        private void UpdateEnemyCountEvent()
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