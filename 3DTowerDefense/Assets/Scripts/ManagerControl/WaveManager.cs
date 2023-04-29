using System;
using Cysharp.Threading.Tasks;
using EnemyControl;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class Wave
        {
            public int enemyCount;
            public string name;
        }

        private bool _startGame;
        private int _curWave;

        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform[] wayPoints;
        [SerializeField] private Transform spawnPoint;

        private void Awake()
        {
            _curWave = -1;
        }

        public void StartGame()
        {
            WaveStart().Forget();
            gameObject.SetActive(false);
        }

        private async UniTaskVoid WaveStart()
        {
            _curWave++;
            var enemyCount = waves[_curWave].enemyCount;
            while (enemyCount > 0)
            {
                await UniTask.Delay(1000);
                enemyCount--;
                SpawnEnemy();
            }

            gameObject.SetActive(true);
        }

        private void SpawnEnemy()
        {
            var e = StackObjectPool.Get<Enemy>(waves[_curWave].name, spawnPoint.position);
            e.CurWayPoint = wayPoints[0];
            e.SetUp();
            e.moveToNextWayPointEvent += SetDestination;
        }

        private void SetDestination(Enemy enemy)
        {
            if (wayPoints[enemy.WayPointIndex++] == wayPoints[^1])
            {
                enemy.gameObject.SetActive(false);
                return;
            }

            enemy.CurWayPoint = wayPoints[enemy.WayPointIndex];
        }
    }
}