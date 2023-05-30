using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerControl
{
    public class WaveManager : MonoBehaviour
    {
        private bool _startGame;
        private int _curWave;
        private int _enemiesIndex;
        private CancellationTokenSource _cts;

        [Serializable]
        public class Wave
        {
            public string enemyName;
            public int enemyCoin;
            public int enemyCount;
            public float atkDelay;
            public int minDamage;
            public int maxDamage;
            public float health;
        }

        [SerializeField] private Wave[] waves;

        public Transform[] WayPointList { get; set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private Button startWaveButton;

        private void Awake()
        {
            startWaveButton.onClick.AddListener(StartWave);
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            _curWave = -1;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
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

            var enemyCount = waves[_curWave].enemyCount;
            while (enemyCount > 0)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
                enemyCount--;
                _enemiesIndex++;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            var e = ObjectPoolManager.Get<EnemyUnit>(waves[_curWave].enemyName,
                WayPointList[0]);

            e.onMoveNextPointEvent += MoveNextPoint;
            e.SetMovePoint(WayPointList[1].position);
            e.onDeadEvent += DeadEnemy;
            e.onCoinEvent += () => uiManager.TowerCoin += waves[_curWave].enemyCoin;
            e.onLifeCountEvent += LifeCount;

            var w = waves[_curWave];
            e.Init(w.minDamage, w.maxDamage, w.atkDelay, w.health);
        }

        private void MoveNextPoint(EnemyUnit enemy)
        {
            var i = ++enemy.wayPointIndex;
            if (i >= WayPointList.Length)
            {
                enemy.gameObject.SetActive(false);
                return;
            }
            enemy.SetMovePoint(WayPointList[i].position);
        }

        private void DeadEnemy()
        {
            _enemiesIndex--;
            if (_enemiesIndex != -1) return;
            print("Stage Complete");
            _startGame = false;
            startWaveButton.gameObject.SetActive(true);
        }

        private void LifeCount()
        {
            uiManager.LifeCount -= 1;
        }

        // [ContextMenu("To Json Data")]
        // private void SaveWaveDateToJson()
        // {
        //     DataManager.SaveDataToJson<WaveData>("waveData.json");
        // }
        //
        // [ContextMenu("From Json Data")]
        // private void LoadWaveDataToJson()
        // {
        //     waveData = DataManager.LoadDataFromJson<WaveData>("waveData.json");
        // }
    }
}