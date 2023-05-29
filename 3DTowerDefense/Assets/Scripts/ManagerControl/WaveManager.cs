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
        private int _enemiesIndex;
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

        [Serializable]
        public class WaveData
        {
            public Wave[] waveData;
        }

        [SerializeField] private WaveData waveData;

        private int _spawnPointIndex;

        public Transform[] SpawnPointList { get; set; }
        public Transform[] DestinationPointList { get; set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private Button startWaveButton;

        private void Awake()
        {
            startWaveButton.onClick.AddListener(StartWave);
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            _curWave = -1;
            _spawnPointIndex = -1;
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
            _enemiesIndex = -1;
            _curWave++;

            var enemyCount = waveData.waveData[_curWave].enemyCount;
            while (enemyCount > 0)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
                enemyCount--;
                _enemiesIndex++;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            _spawnPointIndex++;
            var e = ObjectPoolManager.Get<EnemyUnit>(waveData.waveData[_curWave].enemyName,
                SpawnPointList[_spawnPointIndex]);

            e.SetDestination(DestinationPointList[0]);
            e.onDeadEvent += DeadEnemy;
            e.onCoinEvent += () => uiManager.TowerCoin += waveData.waveData[_curWave].enemyCoin;
            e.onLifeCountEvent += LifeCount;

            var w = waveData.waveData[_curWave];
            e.Init(w.minDamage, w.maxDamage, w.atkDelay, w.health);
            if (_spawnPointIndex == SpawnPointList.Length - 1)
            {
                _spawnPointIndex = -1;
            }
        }

        private void DeadEnemy()
        {
            _enemiesIndex--;
            if (_enemiesIndex != -1) return;
            print("Stage Complete");
            _startGame = false;
            startWaveButton.gameObject.SetActive(true);
        }

        private void LifeCount()
        {
            uiManager.LifeCount -= 1;
        }

        [ContextMenu("To Json Data")]
        private void SaveWaveDateToJson()
        {
            DataManager.SaveDataToJson<WaveData>("waveData.json");
        }

        [ContextMenu("From Json Data")]
        private void LoadWaveDataToJson()
        {
            waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        }
    }
}