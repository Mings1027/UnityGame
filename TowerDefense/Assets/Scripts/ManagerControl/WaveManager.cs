using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using Pathfinding;
using PoolObjectControl;
using UnitControl.EnemyControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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

        private EnemyData[] enemiesData;

        private void Awake()
        {
            _curWave = 0;
            _themeIndex = 0;
            var guids = AssetDatabase.FindAssets("t: EnemyData", new[] { "Assets/EnemyData" });
            enemiesData = new EnemyData[guids.Length];
            for (var i = 0; i < guids.ToArray().Length; i++)
            {
                enemiesData[i] =
                    AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(guids.ToArray()[i]));
            }
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
                for (var j = 0; j < enemiesData.Length; j++)
                {
                    if (enemiesData[j].enemyInfo.startSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }
        }

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPointsArray)
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                for (var j = 0; j < enemiesData.Length; j++)
                {
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray[i], in enemiesData[j]);
                }
            }
        }

        private void EnemyInit(Vector3 wayPoint, in EnemyData enemyData)
        {
            if (enemyData.enemyInfo.startSpawnWave > _curWave) return;

            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.enemyInfo.enemyName, wayPoint);
            enemyUnit.Init(enemyData);

            enemyUnit.TryGetComponent(out EnemyHealth enemyHealth);

            enemyHealth.Init(enemyData.enemyInfo.health);
            enemyHealth.OnUpdateEnemyCountEvent += UpdateEnemyCountEvent;
            var gold = enemyData.enemyInfo.enemyCoin;
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