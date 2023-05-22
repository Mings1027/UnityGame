using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class WaveManager : Singleton<WaveManager>
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

        // [Serializable]
        // public class WaveData
        // {
        //     public Wave[] waves;
        // }
        //
        // private WaveData waveData;

        private int spawnPointIndex;

        public Wave[] waves;
        public event Action<int> onCoinIncreaseEvent;
        public Transform[] SpawnPointList { get; set; }
        public Transform[] DestinationPointList { get; set; }

        private void Start()
        {
            _curWave = -1;
            spawnPointIndex = -1;
            // waveData = new WaveData();
            // if (waveData.waves == null)
            // {
            //     LoadWaveDataToJson();
            // }
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

        public void StartWave()
        {
            WaveStart().Forget();
            gameObject.SetActive(false);
        }

        private async UniTaskVoid WaveStart()
        {
            if (_startGame) return;
            _startGame = true;
            _enemiesIndex = -1;
            _curWave++;

            var enemyCount = waves[_curWave].enemyCount;
            while (enemyCount > 0)
            {
                await UniTask.Delay(1000);
                enemyCount--;
                _enemiesIndex++;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            spawnPointIndex++;
            var e = StackObjectPool.Get<EnemyUnit>(waves[_curWave].enemyName,
                SpawnPointList[spawnPointIndex].position);
            e.GetComponent<Health>().Init(waves[_curWave].health);
            e.onWaveEndedEvent += EndedWave;
            e.SetDestination(DestinationPointList[0]);

            var w = waves[_curWave];
            e.Init(w.minDamage, w.maxDamage, w.atkDelay);
            if (spawnPointIndex == SpawnPointList.Length - 1)
            {
                spawnPointIndex = -1;
            }
        }

        private void EndedWave()
        {
            _enemiesIndex--;
            onCoinIncreaseEvent?.Invoke(waves[_curWave].enemyCoin);
            if (_enemiesIndex != -1) return;
            print("Stage Complete");
            _startGame = false;
            gameObject.SetActive(true);
        }

        // [ContextMenu("From Json Data")]
        // private void LoadWaveDataToJson()
        // {
        //     waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        // }
    }
}