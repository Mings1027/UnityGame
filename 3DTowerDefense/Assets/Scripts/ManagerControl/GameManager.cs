using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject Map { get; set; }
     
        public GameObject UIPrefab { get; private set; }
        public CameraManager Cam => cameraManager;

        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private GameObject mapPrefab;

        private void Awake()
        {
            UIPrefab = Instantiate(gamePlayPrefab);
        }

        private void OnEnable()
        {
            MapInit();
        }

        private void MapInit()
        {
            Map = Instantiate(mapPrefab);
            var waveManager = UIPrefab.GetComponentInChildren<WaveManager>();

            var spawnPoints = Map.transform.Find("Spawn Points");
            waveManager.SpawnPointList = new Transform[spawnPoints.childCount];

            for (var i = 0; i < waveManager.SpawnPointList.Length; i++)
            {
                waveManager.SpawnPointList[i] = spawnPoints.GetChild(i);
            }

            var destinationPoints = Map.transform.Find("Destination Points");
            waveManager.DestinationPointList = new Transform[destinationPoints.childCount];
            for (var i = 0; i < waveManager.DestinationPointList.Length; i++)
            {
                waveManager.DestinationPointList[i] = destinationPoints.GetChild(i);
            }
        }
    }
}