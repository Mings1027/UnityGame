using System;
using Cysharp.Threading.Tasks;
using EnemyControl;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class Wave
        {
            public string name;
            public int enemyCount;
            public float rate;
        }

        private bool _startGame;
        private int _nextWave;
        private int _count;

        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform destinationPoint;

        private void Awake()
        {
            _nextWave = -1;
        }

        public void StartGame()
        {
            WaveStart().Forget();
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

            _startGame = false;
        }

        private void SpawnEnemy()
        {
            StackObjectPool.Get<Enemy>(waves[_nextWave].name, spawnPoint.position).destination = destinationPoint;
        }
    }
}