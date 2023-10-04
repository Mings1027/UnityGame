using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using PoolObjectControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private bool _startWave;
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
            if (_startWave) return;
            _startWave = true;
            _curWave++;

            if (_curWave == 200)
            {
                print("Dragon Is Coming");
                FinalBossWave();
                return;
            }

            if (_curWave % 25 == 0) _themeIndex++;
            TowerManager.Instance.WaveText.text = "Wave : " + _curWave;

            WaveInit(wayPoints.Count);
            SpawnEnemy(wayPoints).Forget();
        }

        private void WaveInit(int wayPointsArrayLength)
        {
            for (var i = 0; i < wayPointsArrayLength; i++)
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

        private void WaveThemeInit(int wayPointsLength)
        {
            for (int i = 0; i < wayPointsLength; i++)
            {
                for (int j = 0; j < waveTheme[i].enemyInfos.Length; j++)
                {
                    if (waveTheme[i].enemyInfos[j].startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }
        }

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPointsArray)
        {
            await UniTask.Delay(500);
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                for (var j = 0; j < waveData.enemyInfos.Length; j++)
                {
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray[i], in waveData.enemyInfos[j]);
                }
            }
        }

        private void EnemyInit(Vector3 wayPoint, in WaveData.EnemyInfo enemyInfo)
        {
            if (enemyInfo.startSpawnWave > _curWave) return;

            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyInfo.enemyName, wayPoint);
            enemyUnit.Init(enemyInfo);

            enemyUnit.TryGetComponent(out EnemyHealth enemyHealth);

            enemyHealth.Init(enemyInfo.health);
            enemyHealth.OnUpdateEnemyCountEvent += UpdateEnemyCountEvent;
            var gold = enemyInfo.enemyCoin;
            enemyHealth.OnDeadEvent += () => TowerManager.Instance.TowerGold += gold;
        }

        private void UpdateEnemyCountEvent()
        {
            if (!_startWave) return;
            _remainingEnemyCount--;
            if (_remainingEnemyCount > 0) return;
            _startWave = false;
            OnPlaceExpandButtonEvent?.Invoke();
            OnEndOfGameEvent?.Invoke();
            TowerManager.Instance.DisableTower();
        }

        private void FinalBossWave()
        {
        }
    }
}