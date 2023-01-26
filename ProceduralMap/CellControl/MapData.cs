using System.Collections.Generic;
using UnityEngine;

namespace CellControl
{
    public class MapData
    {
        public bool[] ObstacleArray;
        public List<KnightPiece> KnightPiecesList;
        public Vector3 StartPosition,ExitPosition;
        public List<Vector3> Path;
    }
}