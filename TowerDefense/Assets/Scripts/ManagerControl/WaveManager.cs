using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.MonsterDataControl;
using InterfaceControl;
using ItemControl;
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

        [SerializeField] private Wave[] monsterData;

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

            SoundManager.PlayBGM(_isBossWave ? SoundEnum.BossTheme : SoundEnum.WaveStart);
            await UniTask.Delay(500);
            _wayPoints = wayPoints;
            _startWave = true;
            StartWave();
        }

        private void StartWave()
        {
            _towerManager.StartTargeting();
            UIManager.instance.waveText.text = curWave.ToString();
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
            var normalMonsterDataLength = monsterData[_themeIndex].normalMonsterData.Length;

            for (var i = 0; i < _themeIndex + 1; i++)
            {
                for (var normalDataIndex = 0; normalDataIndex < normalMonsterDataLength; normalDataIndex++)
                {
                    await UniTask.Delay(500, cancellationToken: _cts.Token);
                    var normalMonsterData = monsterData[_themeIndex].normalMonsterData[normalDataIndex];

                    if (normalMonsterData.StartSpawnWave > curWave) continue;
                    var wayPointCount = wayPoints.Count;

                    for (var wayPoint = 0; wayPoint < wayPointCount; wayPoint++)
                    {
                        var ranPoint = wayPoints[wayPoint] + Random.insideUnitSphere * 3;
                        NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
                        PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
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
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            var ranPoint = ranWayPoint + Random.insideUnitSphere * 3;
            NavMesh.SamplePosition(ranPoint, out var hit, 5, NavMesh.AllAreas);
            PoolObjectManager.Get(PoolObjectKey.TransformSmoke, hit.position);
            Instantiate(monsterData[_themeIndex].bossMonsterData.EnemyPrefab, hit.position,
                Quaternion.identity).TryGetComponent(out GroundBossUnit groundBossUnit);
            BossInit(groundBossUnit, monsterData[_themeIndex].bossMonsterData);
        }

        private void MonsterInit(MonsterUnit monsterUnit, NormalMonsterData normalMonsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(normalMonsterData);
            if (monsterUnit.TryGetComponent(out MonsterStatus monsterStatus))
            {
                monsterStatus.StatInit(normalMonsterData.Speed, normalMonsterData.AttackDelay);
            }

            var healthBarTransform = monsterUnit.healthBarTransform;
            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.MonsterHealthBar,
                healthBarTransform.position);
            if (monsterUnit.TryGetComponent(out Progressive progressive))
            {
                healthBar.Init(progressive);
            }

            monsterUnit.TryGetComponent(out UnitHealth monsterHealth);
            monsterHealth.Init(normalMonsterData.Health);
            StatusBarUIController.Add(healthBar, healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = normalMonsterData.StartSpawnWave;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                        monsterUnit.transform.position + Random.insideUnitSphere)
                    .SetGoldText(coin);
                UIManager.instance.towerGold += coin;

                if (monsterUnit.TryGetComponent(out TransformMonster transformMonster))
                {
                    SpawnTransformMonster(monsterUnit.transform.position, transformMonster);
                }

                StatusBarUIController.Remove(healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit, monsterHealth.IsDead);

                if (monsterHealth.IsDead) return;
                StatusBarUIController.Remove(healthBarTransform, true);
            };
        }

        private void BossInit(MonsterUnit monsterUnit, BossMonsterData bossMonsterData)
        {
            _monsterList.Add(monsterUnit);
            monsterUnit.Init();
            monsterUnit.SpawnInit(bossMonsterData);
            if (monsterUnit.TryGetComponent(out MonsterStatus monsterStatus))
            {
                monsterStatus.StatInit(bossMonsterData.Speed, bossMonsterData.AttackDelay);
            }

            var healthBarTransform = monsterUnit.healthBarTransform;
            var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.BossHealthBar,
                healthBarTransform.position);
            if (monsterUnit.TryGetComponent(out Progressive progressive))
            {
                healthBar.Init(progressive);
            }

            monsterUnit.TryGetComponent(out UnitHealth monsterHealth);
            monsterHealth.Init(bossMonsterData.Health);
            StatusBarUIController.Add(healthBar, healthBarTransform);
            monsterHealth.OnDeadEvent += () =>
            {
                var coin = bossMonsterData.DroppedGold;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText,
                        monsterUnit.transform.position + Random.insideUnitSphere)
                    .SetGoldText(coin);
                UIManager.instance.towerGold += coin;
                StatusBarUIController.Remove(healthBarTransform);
            };
            monsterUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(monsterUnit, monsterHealth.IsDead);

                if (monsterHealth.IsDead) return;
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
                    PoolObjectManager.Get<MonsterUnit>(transformMonster.transformMonsterData.MonsterPoolObjectKey,
                        hit.position);
                MonsterInit(monster, transformMonster.transformMonsterData);
            }
        }

        private void DecreaseEnemyCount(MonsterUnit monsterUnit, bool isDead)
        {
            if (!_startWave) return;
            _monsterList.Remove(monsterUnit);
            var uiManager = UIManager.instance;
            var towerHealth = uiManager.GetTowerHealth();
            if (!isDead) towerHealth.Damage(monsterUnit.baseTowerDamage);

            if (towerHealth.IsDead)
            {
                _pauseController.GameOver();
                return;
            }

            if (_monsterList.Count <= 0)
            {
                _towerManager.StopTargeting();

                if (_monsterList.Count > 0) return;

                WaveStop();
                SoundManager.PlayBGM(SoundEnum.WaveEnd);
                _itemBagController.SetActiveItemBag(false);

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