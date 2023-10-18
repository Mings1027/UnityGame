using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using PoolObjectControl;
using UnitControl.EnemyControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private bool _startWave;
        private bool _isBossWave;
        private sbyte _bossIndex;
        private byte _remainingEnemyCount;
        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyUnits;
        private NavMeshSurface _bossNavmesh;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnEndOfGameEvent;

        [SerializeField, Range(0, 255)] private byte _curWave;
        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private EnemyData[] bossData;

        #region Unity Event

        private void Awake()
        {
            _bossIndex = -1;
            _gameManager = GameManager.Instance;
            _enemyUnits = new List<EnemyUnit>();
            _bossNavmesh = GetComponent<NavMeshSurface>();
            // var enemyDataGuids = AssetDatabase.FindAssets("t: EnemyData", new[] { "Assets/EnemyData" });
            // enemiesData = new EnemyData[enemyDataGuids.Length];
            // for (var i = 0; i < enemyDataGuids.ToArray().Length; i++)
            // {
            //     enemiesData[i] =
            //         AssetDatabase.LoadAssetAtPath<EnemyData>(
            //             AssetDatabase.GUIDToAssetPath(enemyDataGuids.ToArray()[i]));
            // }
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

        #endregion

        public void StartWave(List<Vector3> wayPoints)
        {
            if (_startWave) return;
            _startWave = true;
            _curWave++;

            if (_curWave == 100)
            {
                print("Dragon Is Coming");
                FinalBossWave();
                return;
            }

            // if (_curWave % 15 == 0)
            // {
            //     _isBossWave = true;
            //     _bossIndex++;
            //     BossWave(wayPoints[Random.Range(0, wayPoints.Count)]).Forget();
            //     _bossNavmesh.BuildNavMesh();
            // }

            _gameManager.towerManager.WaveText.text = _curWave.ToString();

            WaveInit(wayPoints.Count);
            SpawnEnemy(wayPoints).Forget();

            EnemyTargeting().Forget();
            EnemyAttack().Forget();
        }

        private void WaveInit(int wayPointsArrayLength)
        {
            for (var i = 0; i < wayPointsArrayLength; i++)
            {
                for (var j = 0; j < enemiesData.Length; j++)
                {
                    if (enemiesData[j].StartSpawnWave <= _curWave)
                    {
                        _remainingEnemyCount++;
                    }
                }
            }

            if (!_isBossWave) return;
            _remainingEnemyCount++;
            _isBossWave = false;
        }

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPointsArray)
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            for (var i = 0; i < wayPointsArray.Count; i++)
            {
                for (var j = 0; j < enemiesData.Length; j++)
                {
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                    EnemyInit(wayPointsArray[i], enemiesData[j]);
                }
            }
        }

        private void EnemyInit(Vector3 wayPoint, EnemyData enemyData)
        {
            if (enemyData.StartSpawnWave > _curWave) return;
            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, wayPoint);
            _enemyUnits.Add(enemyUnit);
            enemyUnit.Init(enemyData);
            enemyUnit.TryGetComponent(out EnemyHealth enemyHealth);
            enemyHealth.Init(enemyData.Health);
            enemyHealth.OnDecreaseEnemyCountEvent += DecreaseEnemyCountEvent;
            enemyHealth.OnDeadEvent += () => _gameManager.towerManager.TowerCost += enemyData.EnemyCoin;
            enemyUnit.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.defaultSpeed = enemyData.Speed;
        }

        private async UniTaskVoid EnemyTargeting()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyUnits.Count; i++)
                {
                    _enemyUnits[i].Targeting();
                }
            }
        }

        private async UniTaskVoid EnemyAttack()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyUnits.Count; i++)
                {
                    _enemyUnits[i].AttackAsync(_cts).Forget();
                }
            }
        }

        private async UniTaskVoid BossWave(Vector3 randomWayPoint)
        {
            await UniTask.Delay(5000, cancellationToken: _cts.Token);
            EnemyInit(randomWayPoint, bossData[_bossIndex]);
        }

        private void DecreaseEnemyCountEvent()
        {
            if (!_startWave) return;
            _remainingEnemyCount--;
            if (_remainingEnemyCount > 0) return;
            _startWave = false;
            OnPlaceExpandButtonEvent?.Invoke();
            _gameManager.towerManager.DisableTower();
            enabled = false;
        }

        private void FinalBossWave()
        {
            //
        }

        public void RemoveEnemyFromList(EnemyUnit enemyUnit) => _enemyUnits.Remove(enemyUnit);
    }
}