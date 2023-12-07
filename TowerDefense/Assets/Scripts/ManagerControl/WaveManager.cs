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

        private Dictionary<MonsterPoolObjectKey, NormalMonsterData> _normalMonsterDataDic;
        private CancellationTokenSource _cts;
        private List<MonsterUnit> _enemyList;
        private Vector3[] _wayPoints;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnBossWaveEvent;
        public static byte curWave { get; private set; }

        [SerializeField] private byte lastWave;

        [SerializeField] private byte bossWaveTerm;

        [SerializeField] private NormalMonsterData[] normalMonstersData;
        [SerializeField] private BossMonsterData[] bossesData;

        #region Unity Event

        private void Awake()
        {
            _bossIndex = 0;
            _enemyList = new List<MonsterUnit>();
            curWave = 0;
            SetMonsterPoolKey();
            _normalMonsterDataDic = new Dictionary<MonsterPoolObjectKey, NormalMonsterData>();
            for (int i = 0; i < normalMonstersData.Length; i++)
            {
                _normalMonsterDataDic.Add(normalMonstersData[i].monsterPoolObjectKey, normalMonstersData[i]);
            }
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
            var enemiesDataLength = normalMonstersData.Length;
            for (var i = 0; i < enemiesDataLength; i++)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                var monsterData = normalMonstersData[i];
                if (monsterData.StartSpawnWave > curWave) continue;
                if (monsterData.IsTransformingMonster)
                {
                    var spawnCount = Random.Range(1, 4);
                    for (var j = 0; j < spawnCount; j++)
                    {
                        var ranWayPoint = Random.Range(0, wayPoints.Count);
                        var ranPoint = wayPoints[ranWayPoint] + Random.insideUnitSphere * 3;
                        NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                        var enemyUnit =
                            PoolObjectManager.Get<MonsterUnit>(normalMonstersData[i].monsterPoolObjectKey,
                                hit.position);
                        EnemyInit(enemyUnit, monsterData);
                    }
                }
                else
                {
                    var wayPointsCount = wayPoints.Count;
                    for (var wayPoint = 0; wayPoint < wayPointsCount; wayPoint++)
                    {
                        var ranPoint = wayPoints[wayPoint] + Random.insideUnitSphere * 3;
                        NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                        var enemyUnit =
                            PoolObjectManager.Get<MonsterUnit>(normalMonstersData[i].monsterPoolObjectKey,
                                hit.position);
                        EnemyInit(enemyUnit, monsterData);
                    }
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
            BossInit(bossUnit, bossesData[_bossIndex]);
            _bossIndex++;
        }

        private void EnemyInit(MonsterUnit monsterUnit, NormalMonsterData normalMonsterData)
        {
            _enemyList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(normalMonsterData);
            monsterUnit.GetComponent<MonsterStatus>().StatInit(normalMonsterData.Speed);

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.EnemyHealthBar,
                monsterUnit.healthBarTransform.position);
            healthBar.Init(monsterUnit.GetComponent<Progressive>());

            var enemyHealth = monsterUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(normalMonsterData.Health);
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, monsterUnit.healthBarTransform);
            enemyHealth.OnDeadEvent += () =>
            {
                var coin = normalMonsterData.StartSpawnWave;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, monsterUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;

                if (!monsterUnit.TryGetComponent(out TransformEnemy transformEnemy)) return;
                var position = monsterUnit.transform.position + Random.insideUnitSphere * 2;
                PoolObjectManager.Get(PoolObjectKey.TransformSmoke, position);
                SpawnTransformEnemy(transformEnemy.SpawnCount, transformEnemy.MonsterPoolObjectKey,
                    position);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                statusUI.Remove(monsterUnit.healthBarTransform);
            };
        }

        private void BossInit(MonsterUnit monsterUnit, BossMonsterData bossMonsterData)
        {
            _enemyList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(bossMonsterData);
            monsterUnit.GetComponent<MonsterStatus>().StatInit(bossMonsterData.Speed);

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.BossHealthBar,
                monsterUnit.healthBarTransform.position);
            healthBar.Init(monsterUnit.GetComponent<Progressive>());

            var enemyHealth = monsterUnit.GetComponent<UnitHealth>();
            enemyHealth.Init(bossMonsterData.Health);
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, monsterUnit.healthBarTransform);
            enemyHealth.OnDeadEvent += () =>
            {
                var coin = (ushort)(bossMonsterData.StartSpawnWave + bossMonsterData.DroppedGold);
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, monsterUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                statusUI.Remove(monsterUnit.healthBarTransform);
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

        private void SpawnTransformEnemy(byte spawnCount, MonsterPoolObjectKey monsterPoolObjectKey, Vector3 spawnPos)
        {
            for (var i = 0; i < spawnCount; i++)
            {
                var enemyUnit = PoolObjectManager.Get<MonsterUnit>(monsterPoolObjectKey, spawnPos);
                EnemyInit(enemyUnit, _normalMonsterDataDic[monsterPoolObjectKey]);
            }
        }

        private void DecreaseEnemyCount(MonsterUnit monsterUnit)
        {
            if (!_startWave) return;
            _enemyList.Remove(monsterUnit);
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

#if UNITY_EDITOR
        [ContextMenu("Set Monster Pool Key")]
        private void SetMonsterPoolKey()
        {
            var index = 0;
            foreach (var value in Enum.GetValues(typeof(MonsterPoolObjectKey)))
            {
                if (index >= normalMonstersData.Length) break;
                normalMonstersData[index].monsterPoolObjectKey = (MonsterPoolObjectKey)value;
                index++;
            }
        }
#endif
    }
}