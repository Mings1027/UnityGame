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
        private CancellationTokenSource cts;

        [Serializable]
        public class Wave
        {
            public string enemyName;
            public int enemyCoin;
            public int enemyCount;
            public float atkDelay;
            public int minDamage;
            public int maxDamage;
            public int health;
        }

        [Serializable]
        public class WaveData
        {
            public Wave[] waveData;
        }

        [SerializeField] private WaveData waveData;

        private int spawnPointIndex;

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
            _curWave = -1;
            spawnPointIndex = -1;
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            cts?.Cancel();
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
                await UniTask.Delay(1000, cancellationToken: cts.Token);
                enemyCount--;
                _enemiesIndex++;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            spawnPointIndex++;
            var e = StackObjectPool.Get<EnemyUnit>(waveData.waveData[_curWave].enemyName,
                SpawnPointList[spawnPointIndex].position);
            e.GetComponent<Health>().Init(waveData.waveData[_curWave].health);
            e.onWaveEndedEvent += IsEndedWave;
            e.SetDestination(DestinationPointList[0]);

            var w = waveData.waveData[_curWave];
            e.Init(w.minDamage, w.maxDamage, w.atkDelay);
            if (spawnPointIndex == SpawnPointList.Length - 1)
            {
                spawnPointIndex = -1;
            }
        }

        private void IsEndedWave()
        {
            _enemiesIndex--;
            uiManager.TowerCoin += waveData.waveData[_curWave].enemyCoin;
            if (_enemiesIndex != -1) return;
            print("Stage Complete");
            _startGame = false;
            startWaveButton.gameObject.SetActive(true);
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