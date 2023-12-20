using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using MonsterControl;
using PoolObjectControl;
using StatusControl;
using TextControl;
using UIControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    [Serializable]
    public class Wave
    {
        public NormalMonsterData[] normalMonsterData;
        public byte startBossWave;
        public BossMonsterData bossMonsterData;
    }

    public class WaveManager : MonoBehaviour
    {
        private TowerManager _towerManager;
        private bool _startWave;
        private bool _isLastWave;
        private bool _isBossWave;
        private byte _themeIndex;
        private bool _isBonusMap;

        private CancellationTokenSource _cts;
        private List<MonsterUnit> _monsterList;
        private Vector3[] _wayPoints;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnBossWaveEvent;
        public static byte curWave { get; private set; }

        [SerializeField] private Wave[] monsterData;

        #region Unity Event

        private void Awake()
        {
            _monsterList = new List<MonsterUnit>();
            curWave = 0;
            _themeIndex = 0;
        }

        private void Start()
        {
            _towerManager = FindObjectOfType<TowerManager>();
        }

        private void Update()
        {
            var leftEnemyCount = _monsterList.Count;
            for (var i = leftEnemyCount - 1; i >= 0; i--)
            {
                _monsterList[i].MonsterUpdate();
            }
        }

        private void OnDisable()
        {
            _monsterList.Clear();
            if (_cts == null) return;
            if (_cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        #endregion

        public void WaveInit()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            PoolObjectManager.PoolCleaner().Forget();
            if (_isBossWave)
            {
                _isBossWave = false;
                PoolObjectManager.MonsterPoolCleaner(monsterData[_themeIndex].normalMonsterData).Forget();
                _themeIndex++;
            }

            curWave++;
            _isBossWave = curWave == monsterData[_themeIndex].startBossWave;
            _isLastWave = _themeIndex == monsterData.Length - 1 && _isBossWave;

            SoundManager.Instance.PlayBGM(_isBossWave ? SoundEnum.BossTheme : SoundEnum.WaveStart);
        }

        public void WaveStart(Vector3[] wayPoints)
        {
            _wayPoints = wayPoints;
            _startWave = true;
            StartWave();
        }

        public void BonusMap()
        {
            Array.Clear(_wayPoints, 0, _wayPoints.Length);
            for (var i = 0; i < 50; i++)
            {
                if (NavMesh.SamplePosition(transform.position, out var hit, 200, NavMesh.AllAreas))
                {
                    _wayPoints[i] = hit.position;
                }
            }

            UIManager.Instance.BonusMap();
            _startWave = true;
            _isLastWave = true;
            StartWave();
        }

        private void StartWave()
        {
            _towerManager.StartTargeting();
            UIManager.Instance.WaveText.text = curWave.ToString();
            SpawnEnemy(_wayPoints).Forget();
            if (_isBossWave)
            {
                SpawnBoss(_wayPoints[Random.Range(0, _wayPoints.Length)]).Forget();
            }

            enabled = true;
        }

        private void WaveStop()
        {
            _startWave = false;
            enabled = false;
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(Vector3[] wayPoints)
        {
            var normalMonsterDataLength = monsterData[_themeIndex].normalMonsterData.Length;
            for (int i = 0; i < _themeIndex + 1; i++)
            {
                for (var normalDataIndex = 0; normalDataIndex < normalMonsterDataLength; normalDataIndex++)
                {
                    await UniTask.Delay(500, cancellationToken: _cts.Token);
                    var normalMonsterData = monsterData[_themeIndex].normalMonsterData[normalDataIndex];
                    if (normalMonsterData.StartSpawnWave > curWave) continue;
                    var wayPointCount = wayPoints.Length;
                    for (var wayPoint = 0; wayPoint < wayPointCount; wayPoint++)
                    {
                        var ranPoint = wayPoints[wayPoint] + Random.insideUnitSphere * 3;
                        NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                        var monster =
                            PoolObjectManager.Get<MonsterUnit>(normalMonsterData.MonsterPoolObjectKey, hit.position);
                        MonsterInit(monster, normalMonsterData);
                    }
                }
            }
        }

        private async UniTaskVoid SpawnBoss(Vector3 ranWayPoint)
        {
            OnBossWaveEvent?.Invoke();
            // UIManager.Instance.UpgradeTowerData();
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            var ranPoint = ranWayPoint + Random.insideUnitSphere * 3;
            NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
            var bossMonster = Instantiate(monsterData[_themeIndex].bossMonsterData.EnemyPrefab, hit.position,
                Quaternion.identity).GetComponent<GroundBossUnit>();
            BossInit(bossMonster, monsterData[_themeIndex].bossMonsterData);
        }

        private void MonsterInit(MonsterUnit monsterUnit, NormalMonsterData normalMonsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(normalMonsterData);
            monsterUnit.GetComponent<MonsterStatus>().StatInit(normalMonsterData.Speed, normalMonsterData.AttackDelay);

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.MonsterHealthBar,
                monsterUnit.healthBarTransform.position);
            healthBar.Init(monsterUnit.GetComponent<Progressive>());

            var monsterHealth = monsterUnit.GetComponent<UnitHealth>();
            monsterHealth.Init(normalMonsterData.Health);
            // var statusUI = StatusBarUIController.Instance;
            StatusBarUIController.Add(healthBar, monsterUnit.healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = normalMonsterData.StartSpawnWave;
                // PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                //         monsterUnit.transform.position + Random.insideUnitSphere)
                //     .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;

                if (monsterUnit.TryGetComponent(out TransformMonster transformMonster))
                {
                    SpawnTransformMonster(monsterUnit.transform.position, transformMonster);
                }

                StatusBarUIController.Remove(monsterUnit.healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                if (monsterHealth.IsDead) return;
                StatusBarUIController.Remove(monsterUnit.healthBarTransform, true);
            };
        }

        private void BossInit(MonsterUnit monsterUnit, BossMonsterData bossMonsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(bossMonsterData);
            monsterUnit.GetComponent<MonsterStatus>().StatInit(bossMonsterData.Speed, bossMonsterData.AttackDelay);

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.BossHealthBar,
                monsterUnit.healthBarTransform.position);
            healthBar.Init(monsterUnit.GetComponent<Progressive>());

            var monsterHealth = monsterUnit.GetComponent<UnitHealth>();
            monsterHealth.Init(bossMonsterData.Health);
            // var statusUI = StatusBarUIController.Instance;
            StatusBarUIController.Add(healthBar, monsterUnit.healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = bossMonsterData.DroppedGold;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                        monsterUnit.transform.position + Random.insideUnitSphere)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;
                StatusBarUIController.Remove(monsterUnit.healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                if (monsterHealth.IsDead) return;
                StatusBarUIController.Remove(monsterUnit.healthBarTransform, true);
            };
        }

        #endregion

        private void SpawnTransformMonster(Vector3 pos, TransformMonster transformMonster)
        {
            for (var i = 0; i < transformMonster.SpawnCount; i++)
            {
                var ranPosition = pos + Random.insideUnitSphere * 2;
                NavMesh.SamplePosition(ranPosition, out var hit, 5, NavMesh.AllAreas);
                PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
                var monster =
                    PoolObjectManager.Get<MonsterUnit>(transformMonster.TransformMonsterData.MonsterPoolObjectKey,
                        hit.position);
                MonsterInit(monster, transformMonster.TransformMonsterData);
            }
        }

        private void DecreaseEnemyCount(MonsterUnit monsterUnit)
        {
            if (!_startWave) return;
            _monsterList.Remove(monsterUnit);
            if (_monsterList.Count > 0) return;
            WaveStop();
            _towerManager.StopTargeting();
            DataManager.UpdateSurvivedWave(curWave);
            SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);

            if (_isLastWave)
            {
                UIManager.Instance.GameEnd();
            }
            else
            {
                OnPlaceExpandButtonEvent?.Invoke();
            }
        }
    }
}