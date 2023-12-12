using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using PoolObjectControl;
using StatusControl;
using TextControl;
using UIControl;
using UnitControl.EnemyControl;
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
        private bool _isEndless;
        private bool _isBossWave;
        private byte _themeIndex;

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

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        // private void Update()
        // {
        //     var enemyCount = _monsterList.Count;
        //     for (var i = enemyCount - 1; i >= 0; i--)
        //     {
        //         _monsterList[i].MonsterUpdate();
        //     }
        // }

        private void OnDisable()
        {
            _cts?.Cancel();
            _monsterList.Clear();
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

        public void WaveStart(Vector3[] wayPoints, bool isEndless)
        {
            enabled = true;
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
                SpawnBoss(_wayPoints[Random.Range(0, _wayPoints.Length)]).Forget();
            }

            MonsterUpdate().Forget();
            // CheckStuck().Forget();
        }

        private void WaveStop()
        {
            if (gameObject == null) return;
            enabled = false;
        }

        #region Enemy Spawn

        private async UniTaskVoid SpawnEnemy(Vector3[] wayPoints)
        {
            var normalMonsterDataLength = monsterData[_themeIndex].normalMonsterData.Length;
            for (var i = 0; i < normalMonsterDataLength; i++)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                var normalMonsterData = monsterData[_themeIndex].normalMonsterData[i];
                if (normalMonsterData.StartSpawnWave > curWave) continue;
                var wayPointCount = wayPoints.Length;
                for (var j = 0; j < wayPointCount; j++)
                {
                    var ranPoint = wayPoints[j] + Random.insideUnitSphere * 3;
                    NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                    var monster =
                        PoolObjectManager.Get<MonsterUnit>(normalMonsterData.MonsterPoolObjectKey, hit.position);
                    MonsterInit(monster, normalMonsterData);
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
                Quaternion.identity).GetComponent<BossUnit>();
            BossInit(bossMonster, monsterData[_themeIndex].bossMonsterData);
        }

        private void MonsterInit(MonsterUnit monsterUnit, NormalMonsterData normalMonsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(normalMonsterData);
            monsterUnit.GetComponent<MonsterStatus>().StatInit(normalMonsterData.Speed, normalMonsterData.AttackDelay);

            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.EnemyHealthBar,
                monsterUnit.healthBarTransform.position);
            healthBar.Init(monsterUnit.GetComponent<Progressive>());

            var monsterHealth = monsterUnit.GetComponent<UnitHealth>();
            monsterHealth.Init(normalMonsterData.Health);
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, monsterUnit.healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = normalMonsterData.StartSpawnWave;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, monsterUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;

                if (monsterUnit.TryGetComponent(out TransformMonster transformMonster))
                {
                    SpawnTransformMonster(monsterUnit.transform.position, transformMonster);
                }

                statusUI.Remove(monsterUnit.healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                if (monsterHealth.IsDead) return;
                statusUI.Remove(monsterUnit.healthBarTransform, true);
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
            var statusUI = StatusBarUIController.Instance;
            statusUI.Add(healthBar, monsterUnit.healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = bossMonsterData.DroppedGold;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, monsterUnit.transform.position)
                    .SetCostText(coin);
                UIManager.Instance.TowerCost += coin;
                statusUI.Remove(monsterUnit.healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit);
                if (monsterHealth.IsDead) return;
                statusUI.Remove(monsterUnit.healthBarTransform, true);
            };
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid MonsterUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(10, cancellationToken: _cts.Token);

                var leftEnemyCount = _monsterList.Count;
                for (var i = leftEnemyCount - 1; i >= 0; i--)
                {
                    _monsterList[i].MonsterUpdate();
                }
            }
        }

        #endregion

        private void SpawnTransformMonster(Vector3 pos, TransformMonster transformMonster)
        {
            var ranPosition = pos + Random.insideUnitSphere * 2;
            NavMesh.SamplePosition(ranPosition, out var hit, 5, NavMesh.AllAreas);
            PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
            for (var i = 0; i < transformMonster.SpawnCount; i++)
            {
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
            if (DataManager._isGameOver) return;
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

        // [ContextMenu("Set Monster Pool Key")]
        // private void SetMonsterPoolKey()
        // {
        //     var index = 0;
        //     foreach (var value in Enum.GetValues(typeof(MonsterPoolObjectKey)))
        //     {
        //         if (index >= normalMonstersData.Length) break;
        //         normalMonstersData[index].monsterPoolObjectKey = (MonsterPoolObjectKey)value;
        //         index++;
        //     }
        // }
    }
}