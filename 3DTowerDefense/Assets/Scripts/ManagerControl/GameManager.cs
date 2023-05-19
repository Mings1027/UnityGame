using System;
using DG.Tweening;
using GameControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private Transform spawnPoints;
        private Transform destinationPoints;
        private Sequence _buildPointSequence;

        public GameObject Map { get; private set; }
        public GameObject GamePlay { get; private set; }

        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private GameObject mapPrefab;

        private void Awake()
        {
            GamePlay = Instantiate(gamePlayPrefab);
            Map = Instantiate(mapPrefab);
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
    }
}