using System.Collections.Generic;
using System.Linq;
using CellControl.AStarAlgorithm;
using UnityEngine;

namespace CellControl
{
    public class CandidateMap
    {
        private MapGrid Grid { get; }
        private readonly int _numberOfPieces;
        private bool[] ObstaclesArray { get; set; }
        private Vector3 _startPoint, _exitPoint;
        private List<KnightPiece> _knightPiecesList;
        private List<Vector3> _path = new();

        public CandidateMap(MapGrid grid, int numberOfPieces)
        {
            _numberOfPieces = numberOfPieces;
            Grid = grid;
        }

        public void CreateMap(Vector3 startPosition, Vector3 exitPosition, bool autoRepair = false)
        {
            _startPoint = startPosition;
            _exitPoint = exitPosition;
            ObstaclesArray = new bool[Grid.Width * Grid.Length];
            _knightPiecesList = new List<KnightPiece>();
            RandomlyPlaceKnightPieces(_numberOfPieces);

            PlacesObstacles();
            FindPath();

            if (autoRepair) Repair();
        }

        private void FindPath()
        {
            _path = AStar.GetPath(_startPoint, _exitPoint, ObstaclesArray, Grid);
            // foreach (var position in path)
            // {
            //     Debug.Log(position);
            // }
            
        }

        private bool CheckIfPositionCanBeObstacle(Vector3 position)
        {
            if (position == _startPoint || position == _exitPoint) return false;
            var index = Grid.CalculateIndexFromCoordinates(position.x, position.z);
            return ObstaclesArray[index] == false;
        }

        private void RandomlyPlaceKnightPieces(int numberOfPieces)
        {
            var count = numberOfPieces;
            var knightPlacementTryLimit = 100;
            while (count > 0 && knightPlacementTryLimit > 0)
            {
                var randomIndex = Random.Range(0, ObstaclesArray.Length);
                if (!ObstaclesArray[randomIndex])
                {
                    var coordinates = Grid.CalculateCoordinatesFromIndex(randomIndex);
                    if (coordinates == _startPoint || coordinates == _exitPoint)
                    {
                        continue;
                    }

                    ObstaclesArray[randomIndex] = true;
                    _knightPiecesList.Add(new KnightPiece(coordinates));
                    count--;
                }

                knightPlacementTryLimit--;
            }
        }

        private void PlaceObstaclesForTheKnight(KnightPiece knight)
        {
            foreach (var position in KnightPiece.ListOfPossibleMoves)
            {
                var newPosition = knight.Position + position;
                if (Grid.IsCellValid(newPosition.x, newPosition.z) && CheckIfPositionCanBeObstacle(newPosition))
                {
                    ObstaclesArray[Grid.CalculateIndexFromCoordinates(newPosition.x, newPosition.z)] = true;
                }
            }
        }

        private void PlacesObstacles()
        {
            foreach (var knight in _knightPiecesList)
            {
                PlaceObstaclesForTheKnight(knight);
            }
        }

        public MapData ReturnMapDate()
        {
            return new MapData
            {
                ObstacleArray = ObstaclesArray,
                KnightPiecesList = _knightPiecesList,
                StartPosition = _startPoint,
                ExitPosition = _exitPoint,
                Path = _path
            };
        }

        public List<Vector3> Repair()
        {
            var numberOfObstacle = ObstaclesArray.Count(obstacle => obstacle);
            var listOfObstaclesToRemove = new List<Vector3>();
            if (_path.Count <= 0)
            {
                do
                {
                    var obstacleIndexToRemove = Random.Range(0, numberOfObstacle);
                    for (var i = 0; i < ObstaclesArray.Length; i++)
                    {
                        if (ObstaclesArray[i])
                        {
                            if (obstacleIndexToRemove == 0)
                            {
                                ObstaclesArray[i] = false;
                                listOfObstaclesToRemove.Add(Grid.CalculateCoordinatesFromIndex(i));
                                break;
                            }

                            obstacleIndexToRemove--;
                        }
                    }

                    FindPath();
                } while (_path.Count <= 0);
            }

            foreach (var obstaclePosition in listOfObstaclesToRemove)
            {
                if (_path.Contains(obstaclePosition) == false)
                {
                    var index = Grid.CalculateIndexFromCoordinates(obstaclePosition.x, obstaclePosition.z);
                    ObstaclesArray[index] = true;
                }
            }

            return listOfObstaclesToRemove;
        }
    }
}