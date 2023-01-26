using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShadowTileMap2D))]
public class ShadowEditor : Editor
{
    [SerializeField]
    protected CompositeCollider2D compositeCollider2D;

    [SerializeField]
    protected bool selfShadows = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ShadowTileMap2D generator = (ShadowTileMap2D)target;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate"))
        {
            generator.GenerateTilemapShadowCasters();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Destroy All Children"))
        {
            generator.DestroyAllChildren();
        }


    }
}
