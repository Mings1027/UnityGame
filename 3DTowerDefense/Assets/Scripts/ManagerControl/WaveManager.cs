using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class Wave
        {
            public string enemyName;
            public int enemyCount;
            public float atkDelay;
            public int minDamage;
            public int maxDamage;
            public int health;
        }

        [Serializable]
        public class WaveData
        {
            public Wave[] waves;
        }

        private bool _startGame;
        private int _curWave;
        private int _enemiesIndex;
        private CancellationTokenSource cts;

        private WaveData waveData;

        public Transform SpawnPoint { get; set; }
        public Transform DestinationPoint { get; set; }

        private void Awake()
        {
            _curWave = -1;
            waveData = new WaveData();
            if (waveData.waves == null)
            {
                LoadWaveDataToJson();
            }
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

            var enemyCount = waveData.waves[_curWave].enemyCount;
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
            var e = StackObjectPool.Get<EnemyUnit>(waveData.waves[_curWave].enemyName, SpawnPoint.position);
            e.GetComponent<Health>().Init(waveData.waves[_curWave].health);
            e.destination = DestinationPoint;
            e.Number = _enemiesIndex;
            e.onFinishWaveCheckEvent += CheckWaveFinish;

            var w = waveData.waves[_curWave];
            e.Init(w.minDamage, w.maxDamage, w.atkDelay);
        }

        private void CheckWaveFinish(int num)
        {
            _enemiesIndex--;
            if (_enemiesIndex == -1)
            {
                print("Stage Complete");
                _startGame = false;
            }
        }

        [ContextMenu("From Json Data")]
        private void LoadWaveDataToJson()
        {
            waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        }
    }
}