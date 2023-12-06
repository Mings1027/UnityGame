using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using PoolObjectControl;
using StatusControl;
using TextControl;
using UIControl;
using UnitControl.EnemyControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private TowerManager _towerManager;
        private bool _startWave;
        private bool _isLastWave;
        private bool _isEndless;
        private bool _isBossWave;

        private sbyte _bossIndex; // Increase After Boss Wave

        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyList;
        private Vector3[] _wayPoints;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnBossWaveEvent;
        public static byte curWave { get; private set; }

        [SerializeField] private byte lastWave;

        [SerializeField] private byte bossWaveTerm;

        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private BossData[] bossesData;

        #region Unity Event

        private void Awake()
        {
            _bossIndex = 0;
            _enemyList = new List<EnemyUnit>();
            curWave = 0;
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

        public void WaveInit()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            curWave++;
            _isLastWave = curWave == lastWave;
            if (curWave == bossWaveTerm)
            {
                _isBossWave = true;
                bossWaveTerm += 10;
            }
            // if (curWave % bossWaveTerm == 0) _isBossWave = true;
            SoundManager.Instance.PlayBGM(_isBossWave ? SoundEnum.BossTheme : SoundEnum.WaveStart);
        }

        public void WaveStart(Vector3[] wayPoints, bool isEndless)
        {
            _wayPoints = wayPoints;
            _startWave = true;
            _isEndless = isEndless;
            StartWave();
        }

        private void StartWave()
        {
            _towerManager.StartTargeting();
            UIManager.Instance.WaveText.text = curWave.ToString();
            SpawnEnemy(_wayPoints).Forget();
            if (_isBossWave)
            {
                _isBossWave = false;
                SpawnBoss(_wayPoints[Random.Range(0, _wayPoints.Length)]).Forget();
            }

            EnemyTargeting().Forget();
            EnemyUpdate().Forget();
            CheckStuck().Forget();
        }

        private void WaveStop()
        {
            _cts?.Cancel();
            _enemyList.Clear();
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPoints)
        {
            var enemiesDataLength = enemiesData.Length;
            for (var i = 0; i < enemiesDataLength; i++)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                var enemyData = enemiesData[i];
                if (enemyData.StartSpawnWave > curWave) continue;

                var wayPointsCount = wayPoints.Count;
                for (var wayPoint = 0; wayPoint < wayPointsCount; wayPoint++)
                {
                    var ranPoint = wayPoints[wayPoint] + Random.insideUnitSphere * 3;
                    NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                    enemyData.EnemyPrefab.TryGetComponent(out EnemyPoolObject enemyPoolObject);
                    var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyPoolObject.enemyPoolObjKey, hit.position);
                    EnemyInit(enemyUnit, enemyData);
                }
            }
        }

        private async UniTaskVoid SpawnBoss(Vector3 ranWayPoint)
        {
            OnBossWaveEvent?.Invoke();
            UIManager.Instance.UpgradeTowerData();
            await UniTask.Delay(2000, cancellationToken: _cts.Token);

            var ranPoint = ranWayPoint + Random.insideUnitSphere * 3;
            NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
            var bossUnit = Instantiate(bossesData[_bossIndex].EnemyPrefab, hit.position, Quaternion.identity)
                .GetComponent<BossUnit>();
            BossInit(bossUnit, (BossData)bossUnit.EnemyData);
            _bossIndex++;
        }

        private void EnemyInit(EnemyUnit enemyUnit, EnemyData enemyData)
        {
            _enemyList.Add(enemyUnit);
            enemyUnit.Init();
            enemyUnit.SpawnInit(enemyData);
            enemyUnit.GetComponent<EnemyStatus>().defaultSpeed = enemyData.Speed;

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.EnemyHealthBar,
                enemyUnit.healthBarTransform.position);
            healthBar.Init(enemyUnit.GetComponent<Progressive>());

            var enemyHealth = enemyUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(enemyData.Health);
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, enemyUnit.healthBarTransform);
            enemyHealth.OnDeadEvent += () =>
            {
                var coin = enemyData.StartSpawnWave;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, enemyUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;
                // statusUI.Remove(enemyUnit.healthBarTransform);

                if (!enemyUnit.TryGetComponent(out TransformEnemy transformEnemy)) return;
                var position = enemyUnit.transform.position;
                PoolObjectManager.Get(PoolObjectKey.TransformSmoke, position);
                SpawnTransformEnemy(transformEnemy.SpawnCount, transformEnemy.EnemyPoolObjectKey,
                    position);
            };
            enemyUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(enemyUnit);
                // if (!enemyHealth.IsDead)
                statusUI.Remove(enemyUnit.healthBarTransform);
            };
        }

        private void BossInit(EnemyUnit enemyUnit, BossData bossData)
        {
            _enemyList.Add(enemyUnit);
            enemyUnit.Init();
            enemyUnit.SpawnInit(bossData);
            enemyUnit.GetComponent<EnemyStatus>().defaultSpeed = bossData.Speed;

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.BossHealthBar,
                enemyUnit.healthBarTransform.position);
            healthBar.Init(enemyUnit.GetComponent<Progressive>());

            var enemyHealth = enemyUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(bossData.Health);
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, enemyUnit.healthBarTransform);
            enemyHealth.OnDeadEvent += () =>
            {
                var coin = (ushort)(bossData.StartSpawnWave + bossData.DroppedGold);
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, enemyUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;
                // statusUI.Remove(enemyUnit.healthBarTransform);
            };
            enemyUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(enemyUnit);
                // if (!enemyHealth.IsDead)
                statusUI.Remove(enemyUnit.healthBarTransform);
            };
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid EnemyTargeting()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                var enemyCount = _enemyList.Count;
                for (var i = enemyCount - 1; i >= 0; i--)
                {
                    _enemyList[i].EnemyTargeting();
                }
            }
        }

        private async UniTaskVoid EnemyUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(250, cancellationToken: _cts.Token);
                var enemyCount = _enemyList.Count;
                for (var i = enemyCount - 1; i >= 0; i--)
                {
                    _enemyList[i].UnitUpdate(_cts);
                }
            }
        }

        private async UniTaskVoid CheckStuck()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(5000, cancellationToken: _cts.Token);

                var leftEnemyCount = _enemyList.Count;
                for (var i = leftEnemyCount - 1; i >= 0; i--)
                {
                    _enemyList[i].IfStuck(_cts).Forget();
                }
            }
        }

        #endregion

        private void SpawnTransformEnemy(byte spawnCount, EnemyPoolObjectKey enemyPoolObjectKey, Vector3 spawnPos)
        {
            for (var i = 0; i < spawnCount; i++)
            {
                var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyPoolObjectKey, spawnPos);
                EnemyInit(enemyUnit, enemyUnit.EnemyData);
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