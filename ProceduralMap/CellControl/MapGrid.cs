using System.Text;
using UnityEngine;

namespace CellControl
{
    public class MapGrid
    {
        public int Width { get; }
        public int Length { get; }
        public Cell[,] CellGrid { get; set; }

        public MapGrid(int width, int length)
        {
            Width = width;
            Length = length;
            CreateGrid();
        }

        private void CreateGrid()
        {
            CellGrid = new Cell[Length, Width];
            for (int row = 0; row < Length; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    CellGrid[row, col] = new Cell(col, row);
                }
            }
        }

        public void SetCell(int x, int z, CellObjectType cellObjectType, bool isTaken = false)
        {
            CellGrid[z, x].ObjectType = cellObjectType;
            CellGrid[z, x].IsTaken = isTaken;
        }

        public void SetCell(float x, float z, CellObjectType cellObjectType, bool isTaken = false)
        {
            CellGrid[(int)x, (int)z].ObjectType = cellObjectType;
            CellGrid[(int)x, (int)z].IsTaken = isTaken;
        }

        public bool IsCellTaken(int x, int z)
        {
            return CellGrid[z, x].IsTaken;
        }

        public bool IsCellTaken(float x, float z)
        {
            return CellGrid[(int)z, (int)x].IsTaken;
        }

        public bool IsCellValid(float x, float z)
        {
            if (x >= Width || x < 0 || z >= Length || z < 0)
            {
                return false;
            }

            return true;
        }

        public Cell GetCell(int x, int z)
        {
            if (!IsCellValid(x, z)) return null;
            return CellGrid[z, x];
        }

        public Cell GetCell(float x, float z)
        {
            return GetCell((int)x, (int)z);
        }

        public int CalculateIndexFromCoordinates(int x, int z)
        {
            return x + z * Width;
        }

        public Vector3 CalculateCoordinatesFromIndex(int randomIndex)
        {
            var x = randomIndex % Width;
            var z = randomIndex / Width;
            return new Vector3(x, 0, z);
        }

        public int CalculateIndexFromCoordinates(float x, float z)
        {
            return (int)x + (int)z * Width;
        }

        public void CheckCoordinates()
        {
            for (int i = 0; i < CellGrid.GetLength(0); i++)
            {
                var b = new StringBuilder();
                for (int j = 0; j < CellGrid.GetLength(0); j++)
                {
                    b.Append(j + "," + i + " ");
                }

                Debug.Log(b.ToString());
            }
        }
    }
}