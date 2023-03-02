using System;
using Cysharp.Threading.Tasks;
using EnemyControl;
using GameControl;
using UnityEngine;
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
            public int health;
            public int damage;
            public float rate;
        }

        private bool _startGame;
        private int _nextWave;
        private int _count;

        [SerializeField] private Button startWaveButton;
        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform destinationPoint;

        private void Awake()
        {
            _nextWave = -1;
        }

        private void Start()
        {
            startWaveButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            WaveStart().Forget();
            startWaveButton.gameObject.SetActive(false);
        }

        private async UniTaskVoid WaveStart()
        {
            if (_startGame) return;
            _startGame = true;
            _nextWave++;
            _count = waves[_nextWave].enemyCount;
            while (_count > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(waves[_nextWave].rate));
                _count--;
                SpawnEnemy();
            }

            startWaveButton.gameObject.SetActive(true);

            _startGame = false;
        }

        private void SpawnEnemy()
        {
            var e = StackObjectPool.Get<Enemy>(waves[_nextWave].name, spawnPoint.position);
            e.GetComponent<Health>().InitializeHealth(waves[_nextWave].health);
            e.destination = destinationPoint;
            e.damage = waves[_nextWave].damage;
        }
    }
}