using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class WaveManager : Singleton<WaveManager>
    {
        private bool _startWave;
        private bool _isLastWave;
        private bool _isBossWave;
        private sbyte _enemyLevel; // Increase After Boss Wave
        private byte _enemyDataIndex; // 

        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyList;
        private NavMeshSurface _bossNavmesh;

        public event Action OnPlaceExpandButtonEvent;
        // public event Action OnEndOfGameEvent;

        [SerializeField, Range(0, 255)] private byte curWave;
        [SerializeField] private byte lastWave;
        [SerializeField] private byte bossWaveTerm;
        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private EnemyData[] bossData;

        #region Unity Event

        protected override void Awake()
        {
            _enemyLevel = 1;
            _enemyDataIndex = 1;
            _enemyList = new List<EnemyUnit>();
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
            _enemyList.Clear();
        }

        #endregion

        public void WaveInit(Vector3[] wayPoints)
        {
            if (_startWave) return;
            enabled = true;
            _startWave = true;
            curWave++;
            if (curWave % 5 == 0 && _enemyDataIndex < enemiesData.Length) _enemyDataIndex++;
            if (curWave % bossWaveTerm == 0)
            {
                _isBossWave = true;
                _bossNavmesh.BuildNavMesh();
            }

            _isLastWave = curWave == lastWave;
            StartWave(ref wayPoints);
        }

        private void StartWave(ref Vector3[] wayPoints)
        {
            TowerManager.Instance.StartTargeting();
            UIManager.Instance.WaveText.text = curWave.ToString();
            SpawnEnemy(wayPoints).Forget();

            if (_isBossWave)
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
            // IsArrived().Forget();
            IfStuck().Forget();
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPoints)
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            for (var i = 0; i < wayPoints.Count; i++)
            {
                for (var j = 0; j < _enemyDataIndex; j++)
                {
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                    EnemyInitTest(wayPoints[i], enemiesData[j]);
                }
            }
        }

        // private void EnemyInit(Vector3 wayPoint, EnemyData enemyData)
        // {
        //     var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, wayPoint + Random.insideUnitSphere);
        //     _enemyList.Add(enemyUnit);
        //     enemyUnit.Init(enemyData);
        //     enemyUnit.OnArrivedBaseTower += () => UIManager.Instance.BaseTowerHealth.Damage(1);
        //     enemyUnit.TryGetComponent(out Health enemyHealth);
        //     enemyHealth.Init(enemyData.Health * _enemyLevel);
        //     enemyHealth.OnDeadEvent += () => UIManager.Instance.TowerCost += enemyData.EnemyCoin * _enemyLevel;
        //     enemyUnit.TryGetComponent(out EnemyStatus enemyStatus);
        //     enemyStatus.defaultSpeed = enemyData.Speed;
        // }

        private void EnemyInitTest(Vector3 wayPoint, EnemyData enemyData)
        {
            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, wayPoint + Random.insideUnitSphere);
            _enemyList.Add(enemyUnit);
            enemyUnit.Init(enemyData);
            enemyUnit.GetComponent<EnemyStatus>().defaultSpeed = enemyData.Speed;
            enemyUnit.OnArrivedBaseTower += () =>
            {
                UIManager.Instance.BaseTowerHealth.Damage(1);
                DecreaseEnemyCount(enemyUnit);
            };

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.EnemyHealthBar);
            healthBar.Init(enemyUnit.GetComponent<Progressive>());

            var enemyHealth = enemyUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(enemyData.Health * _enemyLevel);

            enemyHealth.OnDeadEvent += () =>
            {
                UIManager.Instance.TowerCost += enemyData.EnemyCoin * _enemyLevel;
                DecreaseEnemyCount(enemyUnit);
            };
            enemyUnit.OnDisableEvent += () =>
            {
                ProgressBarUIController.Remove(enemyUnit.HealthBarTransform);
                healthBar.RemoveEvent();
            };

            ProgressBarUIController.Add(healthBar, enemyUnit.HealthBarTransform);
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid EnemyTargeting()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyList.Count; i++)
                {
                    _enemyList[i].Targeting();
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid EnemyAttack()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyList.Count; i++)
                {
                    _enemyList[i].AttackAsync(_cts).Forget();
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid IsArrived()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                for (var i = 0; i < _enemyList.Count; i++)
                {
                    if (!_enemyList[i].gameObject.activeSelf)
                    {
                        DecreaseEnemyCount(_enemyList[i]);
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
                for (var i = 0; i < _enemyList.Count; i++)
                {
                    if (Vector3.Distance(_enemyList[i].prevPos, _enemyList[i].transform.position) <= 5)
                    {
                        _enemyList[i].ResetNavmesh().Forget();
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
                EnemyInitTest(ranPoint, bossData[0]);
            }

            _isBossWave = false;
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
                EnemyInitTest(ranPoint, bossData[0]);
            }
        }

        private void DecreaseEnemyCount(EnemyUnit enemyUnit)
        {
            if (!_startWave) return;
            _enemyList.Remove(enemyUnit);
            if (_enemyList.Count > 0) return;
            _startWave = false;
            OnPlaceExpandButtonEvent?.Invoke();
            TowerManager.Instance.StopTargeting();
            SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);
            enabled = false;

            if (!_isLastWave) return;
            UIManager.Instance.GameEnd();
        }
    }
}