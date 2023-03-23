using System;
using AttackControl;
using Cysharp.Threading.Tasks;
using EnemyControl;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class Wave
        {
            public string name;
            public int enemyCount;

            public float atkDelay;

            public int minDamage;
            public int maxDamage;
            public float atkRange;
            public int health;
        }

        private bool _startGame;
        private int _nextWave;
        private int _count;
        private Button _startWaveButton;

        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform destinationPoint;

        private void Awake()
        {
            _nextWave = -1;
            _startWaveButton = GetComponent<Button>();
        }

        private void Start()
        {
            _startWaveButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            WaveStart().Forget();
            _startWaveButton.gameObject.SetActive(false);
        }

        private async UniTaskVoid WaveStart()
        {
            if (_startGame) return;
            _startGame = true;
            _nextWave++;
            _count = waves[_nextWave].enemyCount;
            while (_count > 0)
            {
                await UniTask.Delay(1000);
                _count--;
                SpawnEnemy();
            }

            _startWaveButton.gameObject.SetActive(true);

            _startGame = false;
        }

        private void SpawnEnemy()
        {
            var e = StackObjectPool.Get<Enemy>(waves[_nextWave].name, spawnPoint.position);
            e.GetComponent<Health>().InitializeHealth(waves[_nextWave].health);
            e.destination = destinationPoint;

            var w = waves[_nextWave];
            e.GetComponent<TargetFinder>().SetUp(w.minDamage, w.maxDamage, w.atkRange, w.atkDelay, w.health);
        }
    }
}