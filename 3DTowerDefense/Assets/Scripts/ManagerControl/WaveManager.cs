using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private bool _startGame;
        private int _curWave;
        private int _remainingEnemyCount;
        private CancellationTokenSource _cts;

        [Serializable]
        public class Wave
        {
            public string enemyName;
            public int enemyCoin;
            public int enemyCount;
            public float atkDelay;
            public int minDamage;
            public int maxDamage;
            public float health;
        }

        [SerializeField] private Wave[] waves;

        public Transform[] WayPointList { get; set; }

        [FormerlySerializedAs("informationUIController")] [SerializeField]
        private InfoUIController infoUIController;

        [SerializeField] private Button startWaveButton;

        private void Awake()
        {
            startWaveButton.onClick.AddListener(StartWave);
        }

        public void ReStart()
        {
            _curWave = -1;
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

        private void StartWave()
        {
            WaveStart().Forget();
            startWaveButton.gameObject.SetActive(false);
        }

        private async UniTaskVoid WaveStart()
        {
            if (_startGame) return;
            _startGame = true;
            _remainingEnemyCount = 0;
            _curWave++;

            var enemyCount = waves[_curWave].enemyCount;
            while (enemyCount > 0)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
                enemyCount--;
                _remainingEnemyCount++;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            var enemyUnit = ObjectPoolManager.Get<EnemyUnit>(waves[_curWave].enemyName,
                WayPointList[0]);

            enemyUnit.SetMovePoint(WayPointList[1].position);
            enemyUnit.onMoveNextPointEvent += MoveNextPoint;

            var enemyHealth = enemyUnit.GetComponent<Health>();
            enemyHealth.onDeadEvent += DeadEnemy;
            enemyHealth.onIncreaseCoinEvent += () => infoUIController.IncreaseCoin(waves[_curWave].enemyCoin);
            enemyHealth.onDecreaseLifeCountEvent += infoUIController.DecreaseLifeCount;

            var w = waves[_curWave];
            enemyUnit.Init(w.minDamage, w.maxDamage, w.atkDelay, w.health);
        }

        private void MoveNextPoint(int index, EnemyUnit enemy)
        {
            if (index >= WayPointList.Length - 1)
            {
                enemy.gameObject.SetActive(false);
                return;
            }

            enemy.SetMovePoint(WayPointList[index + 1].position);
        }

        private void DeadEnemy()
        {
            _remainingEnemyCount--;
            if (_remainingEnemyCount != 0) return;
            print("Stage Complete");
            _startGame = false;
            startWaveButton.gameObject.SetActive(true);
        }

        // [ContextMenu("To Json Data")]
        // private void SaveWaveDateToJson()
        // {
        //     DataManager.SaveDataToJson<WaveData>("waveData.json");
        // }
        //
        // [ContextMenu("From Json Data")]
        // private void LoadWaveDataToJson()
        // {
        //     waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        // }
    }
}