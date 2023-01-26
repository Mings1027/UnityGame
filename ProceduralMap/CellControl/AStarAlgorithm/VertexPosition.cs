using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellControl.AStarAlgorithm
{
    public class VertexPosition : IEquatable<VertexPosition>, IComparable<VertexPosition>
    {
        public static readonly List<Vector2Int> PossibleNeighbours = new()
        {
            new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        public float TotalCost, EstimatedCost;
        public VertexPosition PreviousVertex = null;
        public Vector3 Position { get; }
        public bool IsTaken { get; }

        public int X => (int)Position.x;
        public int Z => (int)Position.z;

        public VertexPosition(Vector3 position, bool isTaken = false)
        {
            Position = position;
            IsTaken = isTaken;
            EstimatedCost = 0;
            TotalCost = 1;
        }

        public int GetHashCode(VertexPosition obj)
        {
            return obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public bool Equals(VertexPosition other)
        {
            return Position == other.Position;
        }

        public int CompareTo(VertexPosition other)
        {
            if (EstimatedCost < other.EstimatedCost) return -1;
            if (EstimatedCost > other.EstimatedCost) return 1;
            return 0;
        }
    }
}