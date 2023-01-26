using UnityEngine;

namespace CellControl
{
    public class Cell
    {
        public int X { get; }
        public int Z { get; }
        public bool IsTaken { get; set; }
        public CellObjectType ObjectType { get; set; }

        public Cell(int x, int z)
        {
            X = x;
            Z = z;
            ObjectType = CellObjectType.Empty;
            IsTaken = false;
        }
    }

    public enum CellObjectType
    {
        Empty,
        Road,
        Obstacle,
        Start,
        Exit
    }
}