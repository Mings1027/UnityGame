using System.Collections.Generic;
using UnityEngine;

namespace CellControl
{
    public class KnightPiece
    {
        public static List<Vector3> ListOfPossibleMoves = new()
        {
            new Vector3(-1, 0, 2),
            new Vector3(1, 0, 2),
            new Vector3(-1, 0, -2),
            new Vector3(1, 0, -2),
            new Vector3(-2, 0, -1),
            new Vector3(-2, 0, 1),
            new Vector3(2, 0, -1),
            new Vector3(2, 0, 1)
        };

        public Vector3 Position { get; }

        public KnightPiece(Vector3 position)
        {
            Position = position;
        }
    }
}