using UnityEngine;

namespace CellControl
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject groundPrefab;

        public void VisualizeGrid(int width, int length)
        {
            var position = new Vector3(width * 0.5f, 0, length * 0.5f);
            var rotation = Quaternion.Euler(90, 0, 0);
            var board = Instantiate(groundPrefab, position, rotation);
            board.transform.localScale = new Vector3(width, length, 1);
        }
    }
}