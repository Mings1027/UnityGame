using System.Collections.Generic;
using GameControl;
using UnityEngine;

namespace PerlinNoiseControl
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private MeshFilter thisMeshFilter;
        [SerializeField] private MeshRenderer thisMeshRenderer;

        [SerializeField] private Material edgeMat;
        [SerializeField] private MeshFilter edgeMeshFilter;
        [SerializeField] private MeshRenderer edgeMeshRenderer;

        [SerializeField] private Material terrainMat;

        [SerializeField] private GameObject[] treePrefabs;
        [Range(0, 1)] [SerializeField] private float waterLevel = 0.4f;
        [Range(0, 1)] [SerializeField] private float scale = 0.1f;
        [Range(0, 1)] [SerializeField] private float treeNoiseScale = 0.04f;
        [Range(0, 1)] [SerializeField] private float treeDensity = 0.5f;
        [Range(0, 500)] [SerializeField] private int size = 100;

        private Cell[][] _grid;

        public bool drawGizmos;

        private void Start()
        {
            thisMeshFilter = GetComponent<MeshFilter>();
            thisMeshRenderer = GetComponent<MeshRenderer>();
            treePrefabs = new GameObject[size];
            for (var i = 0; i < size; i++)
            {
                treePrefabs[i] = StackObjectPool.Get("Tree", transform.position);
                treePrefabs[i].SetActive(false);
            }
        }


        public void GenerateMap()
        {
            _grid = new Cell[size][];
            for (var i = 0; i < size; i++)
            {
                _grid[i] = new Cell[size]; //_grid Init
            }

            var noiseMap = MakeNoiseMap(); //랜덤 값 뽑아서 
            var fallOffMap = MakeFallOffMap();
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cell = new Cell();
                    var noiseValue = noiseMap[x][y];
                    noiseValue -= fallOffMap[x][y];
                    cell.IsWater = noiseValue < waterLevel;
                    _grid[x][y] = cell;
                }
            }

            DrawTerrainMesh(_grid);
            DrawEdgeMesh(_grid);
            DrawTexture(_grid);
            GenerateTrees(_grid);
        }

        #region MakeMap

        private float[][] MakeNoiseMap()
        {
            var noiseMap = new float[size][];
            for (var i = 0; i < size; i++)
            {
                noiseMap[i] = new float[size]; //noiseMap Init
            }

            var (xOffset, yOffset) = (Random.Range(-10000f, 10000), Random.Range(-10000f, 10000));

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    noiseMap[x][y] = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                }
            }

            return noiseMap;
        }

        private float[][] MakeFallOffMap()
        {
            var fallOffMap = new float[size][];
            for (var i = 0; i < size; i++)
            {
                fallOffMap[i] = new float[size];
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var xv = x / (float)size * 2 - 1;
                    var yv = y / (float)size * 2 - 1;
                    var v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                    fallOffMap[x][y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f * (1 - v), 3f));
                }
            }

            return fallOffMap;
        }

        #endregion

        private void DrawTerrainMesh(IReadOnlyList<Cell[]> grid)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cell = grid[x][y];
                    if (cell.IsWater) continue;
                    var a = new Vector3(x - 0.5f, 0, y + 0.5f);
                    var b = new Vector3(x + 0.5f, 0, y + 0.5f);
                    var c = new Vector3(x - 0.5f, 0, y - 0.5f);
                    var d = new Vector3(x + 0.5f, 0, y - 0.5f);
                    var uvA = new Vector2Int(x / size, y / size);
                    var uvB = new Vector2Int((x + 1) / size, y / size);
                    var uvC = new Vector2Int(x / size, (y + 1) / size);
                    var uvD = new Vector2Int((x + 1) / size, (y + 1) / size);
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

            thisMeshFilter.mesh = mesh;
            if (!gameObject.TryGetComponent<MeshCollider>(out _))
                gameObject.AddComponent<MeshCollider>();
            else
            {
                Destroy(gameObject.GetComponent<MeshCollider>());
                gameObject.AddComponent<MeshCollider>();
            }
        }

        private void DrawEdgeMesh(IReadOnlyList<Cell[]> grid)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cell = grid[x][y];
                    if (cell.IsWater) continue;
                    if (x > 0)
                    {
                        var left = grid[x - 1][y];
                        if (left.IsWater)
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
                        var right = grid[x + 1][y];
                        if (right.IsWater)
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
                        var down = grid[x][y - 1];
                        if (down.IsWater)
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
                        var up = grid[x][y + 1];
                        if (!up.IsWater) continue;
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

            edgeMeshFilter.mesh = mesh;

            edgeMeshRenderer.material = edgeMat;
        }

        private void DrawTexture(IReadOnlyList<Cell[]> grid)
        {
            // var texture = new Texture2D(size, size);
            // var colorMap = new Color[size * size];
            // for (var y = 0; y < size; y++)
            // {
            //     for (var x = 0; x < size; x++)
            //     {
            //         var cell = grid[x][y];
            //         colorMap[y * size + x] =
            //             cell.IsWater ? Color.blue : new Color(153f / 255f, 191f / 255f, 115f / 255f);
            //     }
            // }
            //
            // texture.filterMode = FilterMode.Point;
            // texture.SetPixels(colorMap);
            // texture.Apply();

            thisMeshRenderer.material = terrainMat;
            // thisMeshRenderer.material.mainTexture = texture;
        }

        private void GenerateTrees(IReadOnlyList<Cell[]> grid)
        {
            var noiseMap = new float[size][];
            for (var i = 0; i < size; i++)
            {
                noiseMap[i] = new float[size]; //noiseMap Init
            }

            var (xOffset, yOffset) = (Random.Range(-10000f, 10000), Random.Range(-10000f, 10000));
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                    noiseMap[x][y] = noiseValue;
                }
            }

            if (treePrefabs[0].activeSelf)
            {
                for (var i = 0; i < size; i++)
                {
                    treePrefabs[i].SetActive(false);
                }
            }

            var treeIndex = -1;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cell = grid[x][y];
                    if (cell.IsWater) continue;
                    var v = Random.Range(0, treeDensity);
                    if (noiseMap[x][y] < v)
                    {
                        if (treeIndex >= size - 1) break;
                        treeIndex++;
                        treePrefabs[treeIndex] = StackObjectPool.Get("Tree", new Vector3(x, 0, y),
                            Quaternion.Euler(0, Random.Range(0, 360), 0));
                        treePrefabs[treeIndex].transform.localScale = Vector3.one * Random.Range(0.3f, 0.8f);
                    }
                }
            }
        }


        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cell = _grid[x][y];
                    Gizmos.color = cell.IsWater ? Color.blue : Color.green;
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }
    }
}