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
    public class WaveManager : MonoBehaviour
    {
        private bool _startWave;
        private bool _isLastWave;
        private sbyte _enemyLevel; // Increase After Boss Wave
        private byte _enemyDataIndex; // 

        private CancellationTokenSource _cts;
        private List<EnemyUnit> _enemyList;

        public event Action OnPlaceExpandButtonEvent;
        public bool IsBossWave { get; private set; }
        public bool EndWave { get; set; }
        // public event Action OnEndOfGameEvent;

        [SerializeField, Range(0, 255)] private byte curWave;
        [SerializeField] private byte lastWave;
        [SerializeField] private byte bossWaveTerm;
        [SerializeField] private EnemyData[] enemiesData;
        [SerializeField] private EnemyData[] bossData;

        #region Unity Event

        private void Awake()
        {
            _enemyLevel = 1;
            _enemyDataIndex = 1;
            _enemyList = new List<EnemyUnit>();
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
                IsBossWave = true;
            }

            _isLastWave = curWave == lastWave;

            StartWave(ref wayPoints);
        }

        private void StartWave(ref Vector3[] wayPoints)
        {
            TowerManager.Instance.StartTargeting();
            UIManager.Instance.WaveText.text = curWave.ToString();
            SpawnEnemy(wayPoints).Forget();

            if (IsBossWave)
            {
                if (_isLastWave || EndWave)
                {
                    LastWave(wayPoints).Forget();
                }
                else
                {
                    SpawnBoss(wayPoints).Forget();
                }
            }

            EnemyUpdate().Forget();
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
                    EnemyInit(wayPoints[i], enemiesData[j]);
                }
            }
        }

        private void EnemyInit(Vector3 wayPoint, EnemyData enemyData)
        {
            var enemyUnit = PoolObjectManager.Get<EnemyUnit>(enemyData.EnemyKey, wayPoint);
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
                ProgressBarUIController.Remove(enemyUnit.healthBarTransform);
            };
            enemyUnit.OnDisableEvent += () =>
            {
                DecreaseEnemyCount(enemyUnit);
                if (!enemyHealth.IsDead)
                    ProgressBarUIController.Remove(enemyUnit.healthBarTransform);
            };

            ProgressBarUIController.Add(healthBar, enemyUnit.healthBarTransform);
        }

        #endregion

        #region Enemy Update Loop

        private async UniTaskVoid EnemyUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(250, cancellationToken: _cts.Token);
                for (var i = _enemyList.Count - 1; i >= 0; i--)
                {
                    _enemyList[i].UnitUpdate(_cts);
                }
            }
        }

        private async UniTaskVoid IfStuck()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(5000, cancellationToken: _cts.Token);
                for (int i = _enemyList.Count - 1; i >= 0; i--)
                {
                    _enemyList[i].Stuck();
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
                EnemyInit(ranPoint, bossData[0]);
            }
        }

        private void DecreaseEnemyCount(EnemyUnit enemyUnit)
        {
            if (!_startWave) return;
            _enemyList.Remove(enemyUnit);
            if (_enemyList.Count > 0) return;
            enabled = false;
            OnPlaceExpandButtonEvent?.Invoke();
            _startWave = false;
            TowerManager.Instance.StopTargeting();
            SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);

            if (!_isLastWave && !EndWave) return;
            UIManager.Instance.GameEnd();
        }
    }
}