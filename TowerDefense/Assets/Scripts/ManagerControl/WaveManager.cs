using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using PoolObjectControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private bool _startGame;
        private byte _curWave;
        private byte _themeIndex;
        private byte _remainingEnemyCount;
        private CancellationTokenSource _cts;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnEndOfGameEvent;

        [SerializeField] private WaveData[] waveTheme;
        [SerializeField] private WaveData waveData;

        private void Awake()
        {
            _curWave = 0;
            _themeIndex = 0;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        public void StartWave(List<Vector3> wayPoints)
        {
            if (_startGame) return;
            _startGame = true;
            _curWave++;

            if (_curWave == 200)
            {
                print("Dragon Is Coming");
                return;
            }

            if (_curWave % 25 == 0) _themeIndex++;
            TowerManager.Instance.WaveText.text = "Wave : " + _curWave;

            WaveInit(wayPoints);
            SpawnEnemy(wayPoints).Forget();
        }

        private void WaveInit(ICollection wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                for (var j = 0; j < waveData.enemyInfos.Length; j++)
                {
                    if (waveData.enemyInfos[j].startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }
        }

        private void WaveInitTest(ICollection wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                for (var j = 0; j < waveTheme[_themeIndex].enemyInfos.Length; j++)
                {
                    if (waveTheme[_themeIndex].enemyInfos[j].startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }
        }

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPointsArray)
        {
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                if (i >= wayPointsArray.Count)
                {
                    i = 0;
                }

                for (var j = 0; j < waveData.enemyInfos.Length; j++)
                {
                    await UniTask.Delay(200, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray, i, j);
                }
            }

            enabled = false;
        }

        private void EnemyInit(IReadOnlyList<Vector3> wayPointsArray, int i, int j)
        {
            if (waveData.enemyInfos[j].startSpawnWave > _curWave) return;

            var wave = waveData.enemyInfos[j];

            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(waveData.enemyInfos[j].enemyName, wayPointsArray[i]);
            enemyUnit.Init(wave);

            var enemyHealth = enemyUnit.GetComponent<EnemyHealth>();

            enemyHealth.Init(wave.health);
            enemyHealth.OnDecreaseLifeCountEvent += TowerManager.Instance.DecreaseLifeCountEvent;
            enemyHealth.OnUpdateEnemyCountEvent += UpdateEnemyCountEvent;
            enemyHealth.OnDieEvent +=
                () => TowerManager.Instance.GetGoldFromEnemy(waveData.enemyInfos[j].enemyCoin);
        }

        private void UpdateEnemyCountEvent()
        {
            if (!_startGame) return;
            _remainingEnemyCount--;
            if (_remainingEnemyCount > 0) return;
            _startGame = false;
            OnPlaceExpandButtonEvent?.Invoke();
            OnEndOfGameEvent?.Invoke();
            TowerManager.Instance.DisableTower();
        }
    }
}