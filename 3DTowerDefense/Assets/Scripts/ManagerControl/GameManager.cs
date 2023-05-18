using System;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private WaveManager _waveManager;
        private Transform spawnPoints;
        private Transform destinationPoints;

        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private GameObject mapPrefab;

        private void Awake()
        {
            Instantiate(gamePlayPrefab);
        }

        private void OnEnable()
        {
            _waveManager = WaveManager.Instance;
            MapInit();
        }

        private void MapInit()
        {
            var map = Instantiate(mapPrefab).transform;
            spawnPoints = map.transform.Find("Spawn Points");
            _waveManager.SpawnPointList = new Transform[spawnPoints.childCount];

            for (int i = 0; i < _waveManager.SpawnPointList.Length; i++)
            {
                _waveManager.SpawnPointList[i] = spawnPoints.GetChild(i);
            }

            destinationPoints = map.transform.Find("Destination Points");
            _waveManager.DestinationPointList = new Transform[destinationPoints.childCount];
            for (int i = 0; i < _waveManager.DestinationPointList.Length; i++)
            {
                _waveManager.DestinationPointList[i] = destinationPoints.GetChild(i);
            }
        }
    }
}