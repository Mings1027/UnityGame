using UnityEngine;
using System.Collections.Generic;
// using Pathfinding;

public class Grid : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public Material terrainMaterial;
    public Material edgeMaterial;
    public float waterLevel = .4f;
    public float scale = .1f;
    public float treeNoiseScale = .05f;
    public float treeDensity = .5f;
    public float riverNoiseScale = .06f;
    public int rivers = 5;
    public int size = 100;

    public bool makeTrees;

    private Cell[,] _grid;

    private void Start()
    {
        var noiseMap = new float[size, size];
        var (xOffset, yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        // var falloffMap = new float[size, size];
        // for (var y = 0; y < size; y++)
        // {
        //     for (var x = 0; x < size; x++)
        //     {
        //         var xv = x / (float)size * 2 - 1;
        //         var yv = y / (float)size * 2 - 1;
        //         var v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
        //         falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
        //     }
        // }

        _grid = new Cell[size, size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var noiseValue = noiseMap[x, y];
                // noiseValue -= falloffMap[x, y];
                var cell = new Cell(noiseValue < waterLevel);
                _grid[x, y] = cell;
            }
        }

        // GenerateRivers(grid);
        DrawTerrainMesh(_grid);
        DrawEdgeMesh(_grid);
        DrawTexture(_grid);
        if (!makeTrees) return;
        GenerateTrees(_grid);
    }

    private void GenerateRivers(Cell[,] grids)
    {
        var noiseMap = new float[size][];
        for (var index = 0; index < size; index++)
        {
            noiseMap[index] = new float[size];
        }

        var (xOffset, yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var noiseValue = Mathf.PerlinNoise(x * riverNoiseScale + xOffset, y * riverNoiseScale + yOffset);
                noiseMap[x][y] = noiseValue;
            }
        }

        // var gg = AstarData.active.graphs[0] as GridGraph;
        // gg.center = new Vector3(size / 2f - .5f, 0, size / 2f - .5f);
        // gg.SetDimensions(size, size, 1);
        // AstarData.active.Scan(gg);
        // AstarData.active.AddWorkItem(new AstarWorkItem(_ =>
        // {
        //     for (var y = 0; y < size; y++)
        //     {
        //         for (var x = 0; x < size; x++)
        //         {
        //             GraphNode node = gg.GetNode(x, y);
        //             node.Walkable = noiseMap[x][y] > .4f;
        //         }
        //     }
        // }));
        // AstarData.active.FlushGraphUpdates();

        // for (var i = 0; i < rivers; i++)
        // {
        //     GraphNode start = gg.nodes[Random.Range(16, size - 16)];
        //     GraphNode end = gg.nodes[Random.Range(size * (size - 1) + 16, size * size - 16)];
        //     var path = ABPath.Construct((Vector3)start.position, (Vector3)end.position, result =>
        //     {
        //         foreach (var node in result.path)
        //         {
        //             var x = Mathf.RoundToInt(((Vector3)node.position).x);
        //             var y = Mathf.RoundToInt(((Vector3)node.position).z);
        //             grids[x, y].isWater = true;
        //         }
        //     });
        //     AstarPath.StartPath(path);
        //     AstarPath.BlockUntilCalculated(path);
        // }
    }

    private void DrawTerrainMesh(Cell[,] grids)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var cell = grids[x, y];
                if (cell.isWater) continue;
                var a = new Vector3(x - .5f, 0, y + .5f);
                var b = new Vector3(x + .5f, 0, y + .5f);
                var c = new Vector3(x - .5f, 0, y - .5f);
                var d = new Vector3(x + .5f, 0, y - .5f);
                var uvA = new Vector2(x / (float)size, y / (float)size);
                var uvB = new Vector2((x + 1) / (float)size, y / (float)size);
                var uvC = new Vector2(x / (float)size, (y + 1) / (float)size);
                var uvD = new Vector2((x + 1) / (float)size, (y + 1) / (float)size);
                var v = new[] { a, b, c, b, d, c };
                var uv = new[] { uvA, uvB, uvC, uvB, uvD, uvC };
                for (var k = 0; k < 6; k++)
                {
                    vertices.Add(v[k]);
                    triangles.Add(triangles.Count);
                    uvs.Add(uv[k]);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        gameObject.AddComponent<MeshRenderer>();
    }

    private void DrawEdgeMesh(Cell[,] grids)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var cell = grids[x, y];
                if (cell.isWater) continue;
                if (x > 0)
                {
                    var left = grids[x - 1, y];
                    if (left.isWater)
                    {
                        var a = new Vector3(x - .5f, 0, y + .5f);
                        var b = new Vector3(x - .5f, 0, y - .5f);
                        var c = new Vector3(x - .5f, -1, y + .5f);
                        var d = new Vector3(x - .5f, -1, y - .5f);
                        var v = new[] { a, b, c, b, d, c };
                        for (var k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }

                if (x < size - 1)
                {
                    var right = grids[x + 1, y];
                    if (right.isWater)
                    {
                        var a = new Vector3(x + .5f, 0, y - .5f);
                        var b = new Vector3(x + .5f, 0, y + .5f);
                        var c = new Vector3(x + .5f, -1, y - .5f);
                        var d = new Vector3(x + .5f, -1, y + .5f);
                        var v = new[] { a, b, c, b, d, c };
                        for (var k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }

                if (y > 0)
                {
                    var down = grids[x, y - 1];
                    if (down.isWater)
                    {
                        var a = new Vector3(x - .5f, 0, y - .5f);
                        var b = new Vector3(x + .5f, 0, y - .5f);
                        var c = new Vector3(x - .5f, -1, y - .5f);
                        var d = new Vector3(x + .5f, -1, y - .5f);
                        var v = new[] { a, b, c, b, d, c };
                        for (var k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }

                if (y < size - 1)
                {
                    var up = grids[x, y + 1];
                    if (!up.isWater) continue;
                    var a = new Vector3(x + .5f, 0, y + .5f);
                    var b = new Vector3(x - .5f, 0, y + .5f);
                    var c = new Vector3(x + .5f, -1, y + .5f);
                    var d = new Vector3(x - .5f, -1, y + .5f);
                    var v = new[] { a, b, c, b, d, c };
                    for (var k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        var edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);
        edgeObj.AddComponent<MeshFilter>().mesh = mesh;
        edgeObj.AddComponent<MeshRenderer>().material = edgeMaterial;
    }

    private void DrawTexture(Cell[,] grids)
    {
        var texture = new Texture2D(size, size);
        var colorMap = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var cell = grids[x, y];
                if (cell.isWater)
                    colorMap[y * size + x] = Color.blue;
                else
                    colorMap[y * size + x] = new Color(153 / 255f, 191 / 255f, 115 / 255f);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;
        meshRenderer.material.mainTexture = texture;
        gameObject.AddComponent<MeshCollider>();
    }

    private void GenerateTrees(Cell[,] grids)
    {
        var noiseMap = new float[size, size];
        var (xOffset, yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var cell = grids[x, y];
                if (cell.isWater) continue;
                var v = Random.Range(0f, treeDensity);
                if (!(noiseMap[x, y] < v)) continue;
                var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                var tree = Instantiate(prefab, transform);
                tree.transform.position = new Vector3(x, 0, y);
                tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                // tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
            }
        }
    }
}