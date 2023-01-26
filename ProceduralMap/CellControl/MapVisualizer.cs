using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CellControl
{
    public class MapVisualizer : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private Transform _parent;
        [SerializeField] private Color startColor, exitColor;

        [SerializeField] private GameObject roadStraight, roadTileCorner, tileEmpty, startTile, exitTile;
        [SerializeField] private GameObject[] environmentTiles;
        private readonly Dictionary<Vector3, GameObject> _dictionaryOfObstacle = new();

        private void Awake()
        {
            _parent = transform;
        }

        public void VisualizerMap(MapGrid grid, MapData data, bool visualizeUsingPrefabs)
        {
            if (visualizeUsingPrefabs)
            {
                VisualizeUsingPrefabs(grid, data);
            }
            else
            {
                VisualizeUsingPrimitives(grid, data);
            }
        }

        private void VisualizeUsingPrefabs(MapGrid grid, MapData data)
        {
            for (int i = 0; i < data.Path.Count; i++)
            {
                var position = data.Path[i];
                if (position != data.ExitPosition)
                {
                    grid.SetCell(position.x, position.z, CellObjectType.Road);
                }
            }

            for (int col = 0; col < grid.Width; col++)
            {
                for (int row = 0; row < grid.Length; row++)
                {
                    var cell = grid.GetCell(col, row);
                    var position = new Vector3(cell.X, 0, cell.Z);

                    var index = grid.CalculateIndexFromCoordinates(position.x, position.z);
                    if (data.ObstacleArray[index] && cell.IsTaken == false)
                    {
                        cell.ObjectType = CellObjectType.Obstacle;
                    }

                    switch (cell.ObjectType)
                    {
                        case CellObjectType.Empty:
                            CreateIndicator(position,tileEmpty);
                            break;
                        case CellObjectType.Road:
                            CreateIndicator(position,roadStraight);
                            break;
                        case CellObjectType.Obstacle:
                            var randomIndex = Random.Range(0, environmentTiles.Length);
                            CreateIndicator(position,environmentTiles[randomIndex]);
                            break;
                        case CellObjectType.Start:
                            CreateIndicator(position,startTile);
                            break;
                        case CellObjectType.Exit:
                            CreateIndicator(position,exitTile);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void CreateIndicator(Vector3 position, GameObject prefab, Quaternion rotation= new Quaternion())
        {
            var placementPosition = position + new Vector3(0.5f, 0.5f, 0.5f);
            var element = Instantiate(prefab, placementPosition, rotation);
            element.transform.parent = _parent;
            _dictionaryOfObstacle.Add(position,element);
            
        }


        private void VisualizeUsingPrimitives(MapGrid grid, MapData data)
        {
            PlaceStartAndExitPoints(data);
            for (var i = 0; i < data.ObstacleArray.Length; i++)
            {
                if (data.ObstacleArray[i])
                {
                    var positionOnGrid = grid.CalculateCoordinatesFromIndex(i);
                    if (positionOnGrid == data.StartPosition || positionOnGrid == data.ExitPosition) continue;
                    grid.SetCell(positionOnGrid.x, positionOnGrid.z, CellObjectType.Obstacle);
                    if (PlaceKnightObstacle(data, positionOnGrid)) continue;
                    if (_dictionaryOfObstacle.ContainsKey(positionOnGrid) == false)
                    {
                        CreateIndicator(positionOnGrid, Color.white, PrimitiveType.Cube);
                    }
                }
            }
        }

        private bool PlaceKnightObstacle(MapData data, Vector3 positionOnGrid)
        {
            foreach (var knight in data.KnightPiecesList)
            {
                if (knight.Position == positionOnGrid)
                {
                    CreateIndicator(positionOnGrid, Color.red, PrimitiveType.Cube);
                    return true;
                }
            }

            return false;
        }

        private void PlaceStartAndExitPoints(MapData data)
        {
            CreateIndicator(data.StartPosition, startColor, PrimitiveType.Sphere);
            CreateIndicator(data.ExitPosition, exitColor, PrimitiveType.Sphere);
        }

        private void CreateIndicator(Vector3 position, Color color, PrimitiveType sphere)
        {
            var element = GameObject.CreatePrimitive(sphere);
            _dictionaryOfObstacle.Add(position, element);
            element.transform.position = position + new Vector3(0.5f, 0.5f, 0.5f);
            element.transform.parent = _parent;
            var component = element.GetComponent<Renderer>();
            component.material.SetColor(BaseColor, color);
        }

        public void ClearMap()
        {
            foreach (var obstacle in _dictionaryOfObstacle.Values)
            {
                Destroy(obstacle);
            }

            _dictionaryOfObstacle.Clear();
        }
    }
}