using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.MonsterDataControl;
using InterfaceControl;
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
    public class MonsterThemeData
    {
        public NormalMonsterData[] normalMonsterData;
        public byte startBossWave;
        public BossMonsterData bossMonsterData;
    }

    public class WaveManager : MonoBehaviour, IAddressableObject
    {
        private TowerManager _towerManager;
        private ItemBagController _itemBagController;
        private PauseController _pauseController;
        private CancellationTokenSource _cts;
        private List<MonsterUnit> _monsterList;
        private Vector3[] _wayPoints;

        private bool _startWave;
        private bool _isLastWave;
        private bool _isBossWave;
        private byte _themeIndex;

        public event Action OnPlaceExpandButtonEvent;
        public event Action OnBossWaveEvent;
        public static byte curWave { get; private set; }
        public bool isStartWave { get; private set; }

        [SerializeField] private MonsterThemeData[] monstersData;

#region Unity Event

        private void Start()
        {
            _monsterList = new List<MonsterUnit>();
        }

        private void Update()
        {
            var enemyCount = _monsterList.Count;

            for (var i = enemyCount - 1; i >= 0; i--)
            {
                _monsterList[i].DistanceToBaseTower();
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

        public void Init()
        {
            curWave = 0;
            _themeIndex = 0;

            _towerManager = FindObjectOfType<TowerManager>();
            _itemBagController = FindAnyObjectByType<ItemBagController>();
            _pauseController = FindAnyObjectByType<PauseController>();
            _itemBagController = FindAnyObjectByType<ItemBagController>();
        }

        public async UniTaskVoid WaveInit(Vector3[] wayPoints)
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _itemBagController.SetActiveItemBag(true);

            curWave++;
            _isBossWave = curWave == monstersData[_themeIndex].startBossWave;
            _isLastWave = _themeIndex == monstersData.Length - 1 && _isBossWave;

            SoundManager.PlayBGM(_isBossWave ? SoundEnum.BossTheme : SoundEnum.WaveStart);
            await UniTask.Delay(500);
            _wayPoints = wayPoints;
            _startWave = true;
        }

        public void StartWave()
        {
            _towerManager.StartTargeting();
            GameHUD.SetWaveText(curWave.ToString());
            SpawnEnemy(_wayPoints).Forget();

            if (_isBossWave)
            {
                SpawnBoss(_wayPoints[Random.Range(0, _wayPoints.Length)]).Forget();
            }

            enabled = true;
            isStartWave = true;
            MonsterUpdate().Forget();
        }

        private void WaveStop()
        {
            _startWave = false;
            enabled = false;
            isStartWave = false;
        }

#region Enemy Spawn

        private async UniTaskVoid MonsterUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500);
                var leftEnemyCount = _monsterList.Count;

                for (var i = leftEnemyCount - 1; i >= 0; i--)
                {
                    _monsterList[i].MonsterUpdate();
                }
            }
        }

        private async UniTaskVoid SpawnEnemy(IReadOnlyList<Vector3> wayPoints)
        {
            var normalMonsterDataLength = monstersData[_themeIndex].normalMonsterData.Length;

            for (var i = 0; i < _themeIndex + 1; i++)
            {
                for (var normalDataIndex = 0; normalDataIndex < normalMonsterDataLength; normalDataIndex++)
                {
                    var normalMonsterData = monstersData[_themeIndex].normalMonsterData[normalDataIndex];

                    if (normalMonsterData.startSpawnWave > curWave) continue;
                    var wayPointCount = wayPoints.Count;

                    for (var wayPoint = 0; wayPoint < wayPointCount; wayPoint++)
                    {
                        var ranPoint = wayPoints[wayPoint] + Random.insideUnitSphere * 3;
                        NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                        PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
                        var monster =
                            PoolObjectManager.Get<MonsterUnit>(normalMonsterData.monsterPoolObjectKey, hit.position);
                        MonsterInit(monster, normalMonsterData);
                        NormalMonsterInit(monster, normalMonsterData);
                    }

                    await UniTask.Delay(500, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid SpawnBoss(Vector3 ranWayPoint)
        {
            OnBossWaveEvent?.Invoke();
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            var ranPoint = ranWayPoint + Random.insideUnitSphere * 3;
            NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
            PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
            var monster = PoolObjectManager.Get<MonsterUnit>(
                monstersData[_themeIndex].bossMonsterData.monsterPoolObjectKey, hit.position);
            MonsterInit(monster, monstersData[_themeIndex].bossMonsterData);
            BossMonsterInit(monster, monstersData[_themeIndex].bossMonsterData);
        }

        private void MonsterInit(MonsterUnit monsterUnit, MonsterData monsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(monsterData);
            if (monsterUnit.TryGetComponent(out MonsterStatus monsterStatus))
            {
                monsterStatus.StatInit(monsterData.speed, monsterData.attackDelay);
            }
        }

        private void NormalMonsterInit(MonsterUnit monsterUnit, NormalMonsterData normalMonsterData)
        {
            var healthBarTransform = monsterUnit.healthBarTransform;
            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.MonsterHealthBar,
                healthBarTransform.position);
            if (monsterUnit.TryGetComponent(out Progressive progressive))
            {
                healthBar.Init(progressive);
            }

            monsterUnit.TryGetComponent(out UnitHealth monsterHealth);
            monsterHealth.Init(normalMonsterData.health);
            StatusBarUIController.Add(healthBar, healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var gold = normalMonsterData.startSpawnWave;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                        monsterUnit.transform.position + Random.insideUnitSphere)
                    .SetGoldText(gold);
                GameHUD.IncreaseTowerGold(gold);

                if (monsterUnit.TryGetComponent(out TransformMonster transformMonster))
                {
                    SpawnTransformMonster(monsterUnit.transform.position, transformMonster);
                }

                StatusBarUIController.Remove(healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit, monsterHealth.isDead);

                if (monsterHealth.isDead) return;
                StatusBarUIController.Remove(healthBarTransform, true);
            };
        }

        private void BossMonsterInit(MonsterUnit monsterUnit, BossMonsterData bossMonsterData)
        {
            var healthBarTransform = monsterUnit.healthBarTransform;
            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.BossHealthBar,
                healthBarTransform.position);
            if (monsterUnit.TryGetComponent(out Progressive progressive))
            {
                healthBar.Init(progressive);
            }

            monsterUnit.TryGetComponent(out UnitHealth monsterHealth);
            monsterHealth.Init(bossMonsterData.health);
            StatusBarUIController.Add(healthBar, healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var gold = bossMonsterData.droppedGold;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                        monsterUnit.transform.position + Random.insideUnitSphere)
                    .SetGoldText(gold);
                GameHUD.IncreaseTowerGold(gold);
                StatusBarUIController.Remove(healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit, monsterHealth.isDead);

                if (monsterHealth.isDead) return;
                StatusBarUIController.Remove(healthBarTransform, true);
            };
        }

#endregion

        private void SpawnTransformMonster(Vector3 pos, TransformMonster transformMonster)
        {
            for (var i = 0; i < transformMonster.spawnCount; i++)
            {
                var ranPosition = pos + Random.insideUnitSphere * 2;
                NavMesh.SamplePosition(ranPosition, out var hit, 5, NavMesh.AllAreas);
                PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
                var monster =
                    PoolObjectManager.Get<MonsterUnit>(transformMonster.transformMonsterData.monsterPoolObjectKey,
                        hit.position);
                MonsterInit(monster, transformMonster.transformMonsterData);
                NormalMonsterInit(monster, transformMonster.transformMonsterData);
            }
        }

        private void DecreaseEnemyCount(MonsterUnit monsterUnit, bool isDead)
        {
            if (!_startWave) return;
            _monsterList.Remove(monsterUnit);
            var towerHealth = GameHUD.towerHealth;
            if (!isDead) towerHealth.Damage(monsterUnit.baseTowerDamage);

            if (towerHealth.isDead)
            {
                _pauseController.GameOver();
                return;
            }

            if (_monsterList.Count <= 0)
            {
                _towerManager.StopTargeting();

                if (_monsterList.Count > 0) return;

                WaveStop();
                _itemBagController.SetActiveItemBag(false);
                SoundManager.PlayBGM(SoundEnum.WaveEnd);
                PoolObjectManager.PoolCleaner().Forget();

                if (_isBossWave)
                {
                    _isBossWave = false;
                    PoolObjectManager.MonsterPoolCleaner(monstersData[_themeIndex].normalMonsterData).Forget();
                    _themeIndex++;
                }

                if (_isLastWave)
                {
                    _pauseController.GameEnd();
                }
                else
                {
                    OnPlaceExpandButtonEvent?.Invoke();
                }
            }
        }

        public void AllKill()
        {
            for (var i = 0; i < _monsterList.Count; i++)
            {
                if (_monsterList[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(int.MaxValue);
                }
            }
        }
    }
}