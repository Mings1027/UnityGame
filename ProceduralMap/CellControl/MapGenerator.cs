using UnityEngine;

namespace CellControl
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private GridVisualizer gridVisualizer;
        [SerializeField] private MapVisualizer mapVisualizer;

        [SerializeField] private Direction startEdge, exitEdge;
        [SerializeField] private bool randomPlacement;

        [Range(1, 10)] [SerializeField] private int numberOfPieces;
        [Range(3, 20)] [SerializeField] private int width, length;

        [SerializeField] private bool visualizeUsingPrefabs;
        [SerializeField] private bool autoRepair = true;

        private Vector3 _startPosition, _exitPosition;
        private CandidateMap _map;
        private MapGrid _grid;

        private void Start()
        {
            gridVisualizer.VisualizeGrid(width, length);
            GenerateNewMap();
        }

        public void GenerateNewMap()
        {
            mapVisualizer.ClearMap();

            _grid = new MapGrid(width, length);

            MapHalper.RandomlyChooseAndSetStartAndExit(_grid, ref _startPosition, ref _exitPosition, randomPlacement,
                startEdge, exitEdge);

            _map = new CandidateMap(_grid, numberOfPieces);
            _map.CreateMap(_startPosition, _exitPosition, autoRepair);
            mapVisualizer.VisualizerMap(_grid, _map.ReturnMapDate(), visualizeUsingPrefabs);
        }

        public void TryRepair()
        {
            if (_map == null) return;
            var listOfObstaclesToRemove = _map.Repair();
            if (listOfObstaclesToRemove.Count > 0)
            {
                mapVisualizer.ClearMap();
                mapVisualizer.VisualizerMap(_grid, _map.ReturnMapDate(), visualizeUsingPrefabs);
            }
        }
    }
}