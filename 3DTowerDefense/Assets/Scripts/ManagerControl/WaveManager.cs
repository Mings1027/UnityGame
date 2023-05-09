using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;

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
            public int health;
        }

        private bool _startGame;
        private int _curWave;
        private int _enemiesIndex;
        private CancellationTokenSource cts;

        private EnemyUnit[] _enemies;

        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform destinationPoint;

        private void Awake()
        {
            _curWave = -1;
            var maxEnemiesCount = waves.Select(t => t.enemyCount).Prepend(0).Max();

            _enemies = new EnemyUnit[maxEnemiesCount];
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
            var e = StackObjectPool.Get<EnemyUnit>(waves[_curWave].name, spawnPoint.position);
            e.GetComponent<Health>().Init(waves[_curWave].health);
            e.destination = destinationPoint;
            e.Number = _enemiesIndex;
            e.onFinishWaveCheckEvent += RemoveEnemies;
            _enemies[_enemiesIndex] = e;

            var w = waves[_curWave];
            e.UnitInit(w.minDamage, w.maxDamage, w.atkDelay);
        }

        private void RemoveEnemies(int num)
        {
            _enemies[num] = null;
            if (waves.Length - 1 == _curWave)
            {
                if (_enemies.All(x => x == null))
                {
                    print("Complete");
                }
            }
            else
            {
                gameObject.SetActive(true);
            }

            _startGame = false;
        }
    }
}