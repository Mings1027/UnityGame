using System;
using DG.Tweening;
using GameControl;
using TMPro;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private Transform spawnPoints;
        private Transform destinationPoints;
        private Sequence _buildPointSequence;

        public GameObject Map { get; private set; }
        public GameObject GamePlay { get; private set; }

        private int TowerCoin
        {
            get
            {
                coinText.text = towerCoin.ToString();
                return towerCoin;
            }
            set
            {
                towerCoin = value;
                coinText.text = towerCoin.ToString();
            }
        }

        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private GameObject mapPrefab;

        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int towerCoin;

        [SerializeField] private int[] towerBuildCoin;

        private void Awake()
        {
            GamePlay = Instantiate(gamePlayPrefab);
            Map = Instantiate(mapPrefab);
        }

        private void Start()
        {
            GetCoin(2);
        }

        private void OnEnable()
        {
            MapInit();
        }

        private void MapInit()
        {
            var waveManager = WaveManager.Instance;
            spawnPoints = mapPrefab.transform.Find("Spawn Points");
            waveManager.SpawnPointList = new Transform[spawnPoints.childCount];

            for (int i = 0; i < waveManager.SpawnPointList.Length; i++)
            {
                waveManager.SpawnPointList[i] = spawnPoints.GetChild(i);
            }

            destinationPoints = mapPrefab.transform.Find("Destination Points");
            waveManager.DestinationPointList = new Transform[destinationPoints.childCount];
            for (int i = 0; i < waveManager.DestinationPointList.Length; i++)
            {
                waveManager.DestinationPointList[i] = destinationPoints.GetChild(i);
            }
        }

        private void IncreaseCoin(int amount)
        {
            TowerCoin += amount;
        }

        private int GetCoin(int towerLevel)
        {
            var sum = 0;
            for (var i = 0; i < towerLevel; i++)
            {
                sum += towerBuildCoin[i];
            }

            return sum;
        }
    }
}