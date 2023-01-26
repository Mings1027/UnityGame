using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnemyControl
{
    public class WaveSpawner : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        public static Transform[] WayPoints { get; private set; }

        [SerializeField] private float waitingTime;
        [SerializeField] private float countDown;
        [SerializeField] private int waveIndex;
        [SerializeField] private bool _startWave;

        [SerializeField] private Image countDownImage;
        [SerializeField] private TextMeshProUGUI waveCountDownText;

        private void Awake()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            ResetTimer();

            WayPoints = new Transform[transform.childCount];
            for (var i = 0; i < WayPoints.Length; i++)
            {
                WayPoints[i] = transform.GetChild(i);
            }
        }

        private void Update()
        {
            if (countDown <= 0)
            {
                countDown = waitingTime;
                SpawnWave().Forget();
            }
            else if (countDown > 0 && !_startWave)
            {
                countDown -= Time.deltaTime;
            }

            DisplayTime(countDown);
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        private void ResetTimer()
        {
            countDown = waitingTime;
            countDownImage.fillAmount = 1;
            waveCountDownText.text = "00:00";
        }

        private void DisplayTime(float timeDuration)
        {
            if (timeDuration < 0) timeDuration = 0;
            var minutes = Mathf.FloorToInt(timeDuration / 60);
            var seconds = Mathf.FloorToInt(timeDuration % 60);

            waveCountDownText.text = $"{minutes:00} : {seconds:00}";
        }


        private async UniTaskVoid SpawnWave()
        {
            _startWave = true;
            waveIndex++;
            for (var i = 0; i < waveIndex * 2; i++)
            {
                SpawnEnemy();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _cts.Token);
            }

            _startWave = false;
        }


        private void SpawnEnemy()
        {
            StackObjectPool.Get("Enemy", WayPoints[0].position);
        }
    }
}