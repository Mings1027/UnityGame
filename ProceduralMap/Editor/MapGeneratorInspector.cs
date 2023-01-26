using CellControl;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorInspector : UnityEditor.Editor
    {
        private MapGenerator _map;

        private void OnEnable()
        {
            _map = (MapGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying) return;
            if (GUILayout.Button("Generate New Map"))
            {
                _map.GenerateNewMap();
            }

            if (GUILayout.Button("Repair Map"))
            {
                _map.TryRepair();
            }
        }
    }
}