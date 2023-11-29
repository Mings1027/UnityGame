using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        // private NavMeshSurface _bossNavMeshSurface;
        private TowerManager _towerManager;
        private bool _startWave;
        private bool _isLastWave;
        private bool _isEndless;

        private sbyte _enemyLevel; // Increase After Boss Wave
        private byte _enemyDataIndex; // 

        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyList;
        private Vector3[] _wayPoints;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnBossWaveEvent;
        public bool IsBossWave { get; private set; }
        public static byte curWave { get; private set; }

        [SerializeField] private byte lastWave;
        [SerializeField] private byte bossWaveTerm;
        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private EnemyData[] bossData;

        #region Unity Event

        private void Awake()
        {
            // _bossNavMeshSurface = GetComponent<NavMeshSurface>();
            _enemyLevel = 1;
            _enemyDataIndex = 1;
            _enemyList = new List<EnemyUnit>();
            curWave = 0;
            // var enemyDataGuids = AssetDatabase.FindAssets("t: EnemyData", new[] { "Assets/EnemyData" });
            // enemiesData = new EnemyData[enemyDataGuids.Length];
            // for (var i = 0; i < enemyDataGuids.ToArray().Length; i++)
            // {
            //     enemiesData[i] =
            //         AssetDatabase.LoadAssetAtPath<EnemyData>(
            //             AssetDatabase.GUIDToAssetPath(enemyDataGuids.ToArray()[i]));
            // }
        }

        private void Start()
        {
            _towerManager = FindObjectOfType<TowerManager>();
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

        public void WaveStart(Vector3[] wayPoints, bool isEndless)
        {
            _wayPoints = wayPoints;
            _startWave = true;
            _isEndless = isEndless;
            StartWave();
        }

        private void WaveStop()
        {
            _cts?.Cancel();
            _enemyList.Clear();
        }

        public void WaveInit()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            curWave++;
            if (curWave % 5 == 0 && _enemyDataIndex < enemiesData.Length) _enemyDataIndex++;
            if (curWave % bossWaveTerm == 0)
            {
                IsBossWave = true;
                OnBossWaveEvent?.Invoke();
                UIManager.Instance.UpgradeTowerData();
            }

            _isLastWave = curWave == lastWave;
        }

        private void StartWave()
        {
            _towerManager.StartTargeting();
            UIManager.Instance.WaveText.text = curWave.ToString();
            SpawnEnemy(_wayPoints).Forget();

            if (IsBossWave)
            {
                if (_isLastWave)
                {
                    LastWave(_wayPoints).Forget();
                }
                else
                {
                    SpawnBoss(_wayPoints).Forget();
                }
            }

            EnemyUpdate().Forget();
            CheckStuck().Forget();
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPoints)
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            for (int i = 0; i < _enemyDataIndex; i++)
            {
                for (int j = 0; j < wayPoints.Count; j++)
                {
                    EnemyInit(enemiesData[i], wayPoints[j]);
                }

                await UniTask.Delay(500, cancellationToken: _cts.Token);
            }
        }

        private void EnemyInit(EnemyData enemyData, in Vector3 wayPoint)
        {
            NavMesh.SamplePosition(wayPoint, out var hit, 5, NavMesh.AllAreas);
            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, hit.position);
            _enemyList.Add(enemyUnit);
            enemyUnit.Init();
            enemyUnit.SpawnInit(enemyData);
            enemyUnit.GetComponent<EnemyStatus>().defaultSpeed = enemyData.Speed;

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.EnemyHealthBar,
                enemyUnit.healthBarTransform.position);
            healthBar.Init(enemyUnit.GetComponent<Progressive>());

            var enemyHealth = enemyUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(enemyData.Health * _enemyLevel);

            enemyHealth.OnDeadEvent += () =>
            {
                UIManager.Instance.TowerCost += enemyData.EnemyCoin * _enemyLevel;
                StatusBarUIController.Instance.Remove(enemyUnit.healthBarTransform);
            };
            enemyUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(enemyUnit);
                if (!enemyHealth.IsDead)
                    StatusBarUIController.Instance.Remove(enemyUnit.healthBarTransform);
            };

            StatusBarUIController.Instance.Add(healthBar, enemyUnit.healthBarTransform);
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid EnemyUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(250, cancellationToken: _cts.Token);
                var enemyCount = _enemyList.Count - 1;
                for (var i = enemyCount; i >= 0; i--)
                {
                    _enemyList[i].UnitUpdate(_cts);
                }
            }
        }

        private async UniTaskVoid CheckStuck()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);

                var enemyCount = _enemyList.Count - 1;
                for (int i = enemyCount - 1; i >= 0; i--)
                {
                    _enemyList[i].StorePrevPos();
                }

                await UniTask.Delay(5000, cancellationToken: _cts.Token);

                var leftEnemyCount = _enemyList.Count - 1;
                for (int i = leftEnemyCount - 1; i >= 0; i--)
                {
                    _enemyList[i].IfStuck(_cts).Forget();
                }
            }
        }

        #endregion

        private async UniTaskVoid SpawnBoss(IReadOnlyList<Vector3> wayPoints)
        {
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            for (var i = 0; i < _enemyLevel; i++)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                var ranPoint = wayPoints[Random.Range(0, wayPoints.Count)];
                EnemyInit(bossData[0], ranPoint);
            }

            IsBossWave = false;
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
                EnemyInit(bossData[0], ranPoint);
            }
        }

        private void DecreaseEnemyCount(EnemyUnit enemyUnit)
        {
            if (!_startWave) return;
            _enemyList.Remove(enemyUnit);
            if (_enemyList.Count > 0) return;
            WaveStop();
            _towerManager.StopTargeting();
            DataManager.UpdateSurvivedWave(curWave);

            CheckEndlessWave();

            if (!_isLastWave) return;
            UIManager.Instance.GameEnd();
        }

        private void CheckEndlessWave()
        {
            if (_isEndless)
            {
                WaveInit();
                StartWave();
            }
            else
            {
                _startWave = false;
                if (!_isLastWave)
                {
                    OnPlaceExpandButtonEvent?.Invoke();
                }

                SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);
            }
        }
    }
}