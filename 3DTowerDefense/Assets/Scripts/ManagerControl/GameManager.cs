using DG.Tweening;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private Transform spawnPoints;
        private Transform destinationPoints;
        private Sequence _buildPointSequence;

        public GameObject Map { get; private set; }
        public GameObject GamePlayPrefab { get; private set; }

        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private GameObject mapPrefab;

        [SerializeField] private int[] stageStartCoin;

        private void Awake()
        {
            GamePlayPrefab = Instantiate(gamePlayPrefab);
        }

        private void OnEnable()
        {
            MapInit();
        }

        private void MapInit()
        {
            Map = Instantiate(mapPrefab);
            GamePlayPrefab.GetComponent<UIManager>().TowerCoin = stageStartCoin[0];
            var waveManager = GamePlayPrefab.GetComponentInChildren<WaveManager>();

            spawnPoints = Map.transform.Find("Spawn Points");
            waveManager.SpawnPointList = new Transform[spawnPoints.childCount];

            for (var i = 0; i < waveManager.SpawnPointList.Length; i++)
            {
                waveManager.SpawnPointList[i] = spawnPoints.GetChild(i);
            }

            destinationPoints = Map.transform.Find("Destination Points");
            waveManager.DestinationPointList = new Transform[destinationPoints.childCount];
            for (var i = 0; i < waveManager.DestinationPointList.Length; i++)
            {
                waveManager.DestinationPointList[i] = destinationPoints.GetChild(i);
            }
        }
    }
}