using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EnemyControl;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public struct Wave
        {
            public int enemyCount;
            public string name;
        }

        private bool _startGame;
        private int _curWave;
        private Transform _spawnPoint;

        [SerializeField] private float spawnDelay;
        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform[] wayPoints;

        private void Awake()
        {
            _curWave = -1;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            _curWave = -1;
            _spawnPoint = wayPoints[0];
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
                await UniTask.Delay(TimeSpan.FromSeconds(spawnDelay));
                enemyCount--;
                SpawnEnemy();
            }

            gameObject.SetActive(true);
        }

        private void SpawnEnemy()
        {
            var e = StackObjectPool.Get<Enemy>(waves[_curWave].name, _spawnPoint.position + Vector3.up * 10);
            e.moveToNextWayPointEvent += SetDestination;
            e.transform.DOMoveY(wayPoints[0].position.y, 1).SetEase(Ease.InQuint)
                .OnComplete(() => e.Init(true, wayPoints[0].position, wayPoints[1].position));
        }

        private void SetDestination(Enemy enemy)
        {
            enemy.SetDestination(wayPoints);
        }
    }
}