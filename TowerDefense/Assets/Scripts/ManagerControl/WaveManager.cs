using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class WaveManager : Singleton<WaveManager>
    {
        private bool _startWave;
        private bool _isLastWave;
        private bool isBossWave;
        private sbyte _enemyLevel; // Increase After Boss Wave
        private byte enemyDataIndex; // 
        
        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyUnits;
        private NavMeshSurface bossNavmesh;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnEndOfGameEvent;

        [SerializeField, Range(0, 255)] private byte _curWave;
        [SerializeField] private byte lastWave;
        [SerializeField] private byte bossWaveTerm;
        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private EnemyData[] bossData;

        #region Unity Event

        private void Awake()
        {
            _enemyLevel = 1;
            enemyDataIndex = 1;
            _enemyUnits = new List<EnemyUnit>();
            bossNavmesh = GetComponent<NavMeshSurface>();
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

        public void WaveInit(Vector3[] wayPoints)
        {
            if (_startWave) return;
            enabled = true;
            _startWave = true;
            _curWave++;
            if (_curWave % 5 == 0 && enemyDataIndex < enemiesData.Length) enemyDataIndex++;
            if (_curWave % bossWaveTerm == 0)
            {
                isBossWave = true;
                bossNavmesh.BuildNavMesh();
            }

            _isLastWave = _curWave == lastWave;
            StartWave(ref wayPoints);
        }

        private void StartWave(ref Vector3[] wayPoints)
        {
            TowerManager.Instance.StartTargeting();
            UIManager.Instance.WaveText.text = _curWave.ToString();
            SpawnEnemy(wayPoints).Forget();

            if (isBossWave)
            {
                if (_isLastWave)
                {
                    LastWave(wayPoints).Forget();
                }
                else
                {
                    SpawnBoss(wayPoints).Forget();
                }
            }

            EnemyTargeting().Forget();
            EnemyAttack().Forget();
            IsArrived().Forget();
            IfStuck().Forget();
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPoints)
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            for (var i = 0; i < wayPoints.Count; i++)
            {
                for (var j = 0; j < enemyDataIndex; j++)
                {
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                    EnemyInit(wayPoints[i], enemiesData[j]);
                }
            }
        }

        private void EnemyInit(Vector3 wayPoint, EnemyData enemyData)
        {
            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, wayPoint);
            _enemyUnits.Add(enemyUnit);
            enemyUnit.Init(enemyData);
            enemyUnit.TryGetComponent(out Health enemyHealth);
            enemyHealth.Init(enemyData.Health * _enemyLevel);
            enemyHealth.OnDeadEvent += () => UIManager.Instance.TowerCost += enemyData.EnemyCoin * _enemyLevel;
            enemyUnit.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.defaultSpeed = enemyData.Speed;
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid EnemyTargeting()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyUnits.Count; i++)
                {
                    _enemyUnits[i].Targeting();
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
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
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid IsArrived()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyUnits.Count; i++)
                {
                    if (_enemyUnits[i].IsArrived())
                    {
                        _enemyUnits[i].gameObject.SetActive(false);
                        _enemyUnits[i].StatusInit();
                        UIManager.Instance.BaseTowerHealth.Damage(1);
                        DecreaseEnemyCount(_enemyUnits[i]);
                    }
                    else
                    {
                        if (!_enemyUnits[i].gameObject.activeSelf)
                        {
                            DecreaseEnemyCount(_enemyUnits[i]);
                        }
                    }
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid IfStuck()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(5000, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyUnits.Count; i++)
                {
                    if (Vector3.Distance(_enemyUnits[i].prevPos, _enemyUnits[i].transform.position) <= 5)
                    {
                        print(_enemyUnits[i].name);
                        print(_enemyUnits[i].transform.position);
                        _enemyUnits[i].ResetNavmesh().Forget();
                    }

                    await UniTask.Delay(3000, cancellationToken: _cts.Token);
                }
            }
        }

        #endregion

        private async UniTaskVoid SpawnBoss(IReadOnlyList<Vector3> wayPoints)
        {
            await UniTask.Delay(5000, cancellationToken: _cts.Token);
            for (var i = 0; i < _enemyLevel; i++)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                var ranPoint = wayPoints[Random.Range(0, wayPoints.Count)];
                EnemyInit(ranPoint, bossData[0]);
            }

            isBossWave = false;
            _enemyLevel++;
        }

        private async UniTaskVoid LastWave(IReadOnlyList<Vector3> wayPoints)
        {
            // 마지막 웨이브라고 알려주기
            await UniTask.Delay(5000, cancellationToken: _cts.Token);
            for (var i = 0; i < wayPoints.Count; i++)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                var ranPoint = wayPoints[Random.Range(0, wayPoints.Count)];
                EnemyInit(ranPoint, bossData[0]);
            }
        }

        private void DecreaseEnemyCount(EnemyUnit enemyUnit)
        {
            if (!_startWave) return;
            _enemyUnits.Remove(enemyUnit);
            if (_enemyUnits.Count > 0) return;
            _startWave = false;
            OnPlaceExpandButtonEvent?.Invoke();
            TowerManager.Instance.StopTargeting();
            enabled = false;
        }
    }
}