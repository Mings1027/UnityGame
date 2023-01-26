using System.Collections.Generic;
using UnityEngine;

namespace CellControl.AStarAlgorithm
{
    public static class AStar
    {
        public static List<Vector3> GetPath(Vector3 start, Vector3 exit, bool[] obstaclesArray, MapGrid grid)
        {
            var startVertex = new VertexPosition(start);
            var exitVertex = new VertexPosition(exit);

            var path = new List<Vector3>();

            var openedList = new List<VertexPosition>();
            var closedList = new HashSet<VertexPosition>();

            startVertex.EstimatedCost = ManhattanDistance(startVertex, exitVertex);

            openedList.Add(startVertex);

            while (openedList.Count > 0)
            {
                openedList.Sort();
                var currentVertex = openedList[0];

                if (currentVertex.Equals(exitVertex))
                {
                    while (currentVertex != startVertex)
                    {
                        path.Add(currentVertex.Position);
                        currentVertex = currentVertex.PreviousVertex;
                    }

                    path.Reverse();
                    break;
                }

                var arrayOfNeighbours = FindNeighbourFor(currentVertex, grid, obstaclesArray);
                foreach (var neighbour in arrayOfNeighbours)
                {
                    if (neighbour == null || closedList.Contains(neighbour)) continue;
                    if (neighbour.IsTaken == false)
                    {
                        var totalCost = currentVertex.TotalCost + 1;
                        var neighbourEstimatedCost = ManhattanDistance(neighbour, exitVertex);
                        neighbour.TotalCost = totalCost;
                        neighbour.PreviousVertex = currentVertex;
                        neighbour.EstimatedCost = totalCost + neighbourEstimatedCost;
                        if(openedList.Contains(neighbour)==false) openedList.Add(neighbour);
                    }
                }

                closedList.Add(currentVertex);
                openedList.Remove(currentVertex);
            }

            return path;
        }

        private static VertexPosition[] FindNeighbourFor(VertexPosition currentVertex, MapGrid grid,
            bool[] obstaclesArray)
        {
            var arrayOfNeighbours = new VertexPosition[4];
            var arrayIndex = 0;
            foreach (var possibleNeighbour in VertexPosition.PossibleNeighbours)
            {
                var position = new Vector3(currentVertex.X + possibleNeighbour.x, 0,
                    currentVertex.Z + possibleNeighbour.y);
                if (grid.IsCellValid(position.x, position.z))
                {
                    var index = grid.CalculateIndexFromCoordinates(position.x, position.z);
                    arrayOfNeighbours[arrayIndex] = new VertexPosition(position, obstaclesArray[index]);
                    arrayIndex++;
                }
            }

            return arrayOfNeighbours;
        }

        private static float ManhattanDistance(VertexPosition startVertex, VertexPosition exitVertex)
        {
            return Mathf.Abs(startVertex.X - exitVertex.X) + Mathf.Abs(startVertex.Z - exitVertex.Z);
        }
    }
}
