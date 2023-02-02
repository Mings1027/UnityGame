//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static readonly HashSet<Mesh> RegisteredMeshes = new();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode
    {
        get => outlineMode;
        set
        {
            outlineMode = value;
            _needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            outlineColor = value;
            _needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get => outlineWidth;
        set
        {
            outlineWidth = value;
            _needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField] private Mode outlineMode;

    [SerializeField] private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Optional")]
    [SerializeField, Tooltip(
         "Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
         + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;

    [SerializeField, HideInInspector] private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector] private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] _renderers;
    private Material _outlineMaskMaterial;
    private Material _outlineFillMaterial;

    private bool _needsUpdate;
    private static readonly int OutlineColor1 = Shader.PropertyToID("_OutlineColor");
    private static readonly int ZTest = Shader.PropertyToID("_ZTest");
    private static readonly int Width = Shader.PropertyToID("_OutlineWidth");

    void Awake()
    {
        // Cache renderers
        _renderers = GetComponentsInChildren<Renderer>();

        // Instantiate outline materials
        _outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        _outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        _outlineMaskMaterial.name = "OutlineMask (Instance)";
        _outlineFillMaterial.name = "OutlineFill (Instance)";

        // Retrieve or generate smooth normals
        LoadSmoothNormals();

        // Apply material properties immediately
        _needsUpdate = true;
    }

    private void OnEnable()
    {
        foreach (var rend in _renderers)
        {
            // Append outline shaders
            var materials = rend.sharedMaterials.ToList();

            materials.Add(_outlineMaskMaterial);
            materials.Add(_outlineFillMaterial);

            rend.materials = materials.ToArray();
        }
    }

    private void OnValidate()
    {
        // Update material properties
        _needsUpdate = true;

        // Clear cache when baking is disabled or corrupted
        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        // Generate smooth normals when baking is enabled
        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }

    private void Update()
    {
        if (!_needsUpdate) return;
        _needsUpdate = false;
        UpdateMaterialProperties();
    }

    private void OnDisable()
    {
        foreach (var rend in _renderers)
        {
            // Remove outline shaders
            var materials = rend.sharedMaterials.ToList();

            materials.Remove(_outlineMaskMaterial);
            materials.Remove(_outlineFillMaterial);

            rend.materials = materials.ToArray();
        }
    }

    private void OnDestroy()
    {
        // Destroy material instances
        Destroy(_outlineMaskMaterial);
        Destroy(_outlineFillMaterial);
    }

    private void Bake()
    {
        // Generate smooth normals for each mesh
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            // Skip duplicates
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Serialize smooth normals
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    private void LoadSmoothNormals()
    {
        // Retrieve or generate smooth normals
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            // Skip if smooth normals have already been adopted
            if (!RegisteredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Retrieve or generate smooth normals
            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            // Store smooth normals in UV3
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            // Combine subMeshes
            var rend = meshFilter.GetComponent<Renderer>();

            if (rend != null)
            {
                CombineSubMeshes(meshFilter.sharedMesh, rend.sharedMaterials);
            }
        }

        // Clear UV3 on skinned mesh renderers
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            // Skip if UV3 has already been reset
            if (!RegisteredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                continue;
            }

            // Clear UV3
            var sharedMesh = skinnedMeshRenderer.sharedMesh;
            sharedMesh.uv4 = new Vector2[sharedMesh.vertexCount];

            // Combine subMeshes
            CombineSubMeshes(sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
            .GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups)
        {
            // Skip single vertices
            if (group.Count() == 1)
            {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    private void CombineSubMeshes(Mesh mesh, IReadOnlyCollection<Material> materials)
    {
        // Skip meshes with a single subMesh
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        // Skip if subMesh count exceeds material count
        if (mesh.subMeshCount > materials.Count)
        {
            return;
        }

        // Append combined subMesh
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    private void UpdateMaterialProperties()
    {
        // Apply properties according to mode
        _outlineFillMaterial.SetColor(OutlineColor1, outlineColor);

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                _outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineVisible:
                _outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineHidden:
                _outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                _outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.OutlineAndSilhouette:
                _outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(Width, outlineWidth);
                break;

            case Mode.SilhouetteOnly:
                _outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                _outlineFillMaterial.SetFloat(Width, 0f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}