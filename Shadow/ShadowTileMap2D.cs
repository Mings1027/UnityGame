using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
public static class ShadowCaster2DExtensions
{
    public static void SetPath(this ShadowCaster2D shadowCaster, Vector3[] path)
    {
        FieldInfo shapeField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
        shapeField.SetValue(shadowCaster, path);
    }

    public static void SetPathHash(this ShadowCaster2D shadowCaster, int hash)
    {
        FieldInfo hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
        hashField.SetValue(shadowCaster, hash);
    }
}
public class ShadowTileMap2D : MonoBehaviour
{
    private CompositeCollider2D compositeCollider2D;
    [SerializeField]
    private bool selfShadows = true;

    public void GenerateTilemapShadowCasters()
    {
        // First, it destroys the existing shadow casters
        DestroyAllChildren();

        // Then it creates the new shadow casters, based on the paths of the composite collider
        compositeCollider2D = GetComponent<CompositeCollider2D>();
        List<Vector2> pointsInPath = new List<Vector2>();
        List<Vector3> pointsInPath3D = new List<Vector3>();

        int compositeCollider2D_PathCount = compositeCollider2D.pathCount;
        for (int i = 0; i < compositeCollider2D_PathCount; ++i)
        {
            compositeCollider2D.GetPath(i, pointsInPath);

            GameObject newShadowCaster = new GameObject("ShadowCaster2D_" + i);
            newShadowCaster.isStatic = true;
            newShadowCaster.transform.SetParent(compositeCollider2D.transform, false);

            for (int j = 0; j < pointsInPath.Count; ++j)
            {
                pointsInPath3D.Add(pointsInPath[j]);
            }

            ShadowCaster2D shadowCaster2D = newShadowCaster.AddComponent<ShadowCaster2D>();
            shadowCaster2D.SetPath(pointsInPath3D.ToArray());
            shadowCaster2D.SetPathHash(Random.Range(int.MinValue, int.MaxValue)); // The hashing function GetShapePathHash could be copied from the LightUtility class
            shadowCaster2D.selfShadows = this.selfShadows;
            shadowCaster2D.Update();

            pointsInPath.Clear();
            pointsInPath3D.Clear();
        }
    }

    public void DestroyAllChildren()
    {
        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }
    }












}
