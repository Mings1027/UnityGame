using UnityEditor;
using UnityEngine;

namespace PerlinNoiseControl
{
    [CustomEditor(typeof(Grid))]
    public class GridGeneratorInspector : Editor
    {
        private Grid _grid;

        private void OnEnable()
        {
            _grid = (Grid)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying) return;
            if (GUILayout.Button("Generate New Map"))
            {
                _grid.GenerateMap();
            }
        }
    }
}
