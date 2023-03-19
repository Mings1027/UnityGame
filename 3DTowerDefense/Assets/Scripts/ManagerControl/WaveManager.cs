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

            [FormerlySerializedAs("delay")] [FormerlySerializedAs("rate")]
            public float atkDelay;

            [FormerlySerializedAs("damage")] public int minDamage;
            public int maxDamage;
            public float atkRange;
            public int health;
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
                await UniTask.Delay(TimeSpan.FromSeconds(waves[_nextWave].atkDelay));
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

            var w = waves[_nextWave];
            e.GetComponent<TargetFinder>().SetUp(w.atkDelay, w.atkRange, w.minDamage, w.maxDamage, w.health);
            e.damage = waves[_nextWave].minDamage;
        }
    }
}