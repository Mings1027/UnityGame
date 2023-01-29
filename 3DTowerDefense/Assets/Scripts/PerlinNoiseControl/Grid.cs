using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoiseControl
{
    public class Grid : MonoBehaviour
    {
        private Cell[][] _grid;
        private readonly List<Vector3> _notTreeMap = new();

        public bool drawGizmos;

        private MeshFilter _terrainMeshFilter;
        private MeshRenderer _terrainMeshRenderer;
        private GameObject[] _trees;

        [SerializeField] private Material terrainMat;

        [SerializeField] private Material edgeMat;
        [SerializeField] private MeshFilter edgeMeshFilter;
        [SerializeField] private MeshRenderer edgeMeshRenderer;

        [Range(0, 500)] [SerializeField] private int gridSize = 100;
        [Range(0, 1)] [SerializeField] private float waterLevel = 0.4f;
        [Range(0, 1)] [SerializeField] private float noiseScale = 0.1f;
        [Range(0, 5)] [SerializeField] private float treeNoiseScale = 0.04f;
        [Range(0, 1)] [SerializeField] private float treeDensity = 0.5f;
        [SerializeField] private GameObject crystal;

        private void Awake()
        {
            _terrainMeshFilter = GetComponent<MeshFilter>();
            _terrainMeshRenderer = GetComponent<MeshRenderer>();
            _trees = new GameObject[gridSize];
        }

        private void Start()
        {
            for (var i = 0; i < gridSize; i++)
            {
                _trees[i] = StackObjectPool.Get("Tree", transform.position);
                _trees[i].SetActive(false);
            }
        }


        public void GenerateMap()
        {
            _grid = new Cell[gridSize][];
            for (var i = 0; i < gridSize; i++)
            {
                _grid[i] = new Cell[gridSize]; //_grid Init
            }

            var noiseMap = MakeNoiseMap(); //랜덤 값 뽑아서 
            var fallOffMap = MakeFallOffMap();
            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
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
            GenerateCrystal().Forget();
        }

        #region MakeMap

        private float[][] MakeNoiseMap()
        {
            var noiseMap = new float[gridSize][];
            for (var i = 0; i < gridSize; i++)
            {
                noiseMap[i] = new float[gridSize]; //noiseMap Init
            }

            var (xOffset, yOffset) = (Random.Range(-10000, 10000), Random.Range(-10000, 10000));

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    noiseMap[x][y] = Mathf.PerlinNoise(x * noiseScale + xOffset, y * noiseScale + yOffset);
                }
            }

            return noiseMap;
        }

        private float[][] MakeFallOffMap()
        {
            var fallOffMap = new float[gridSize][];
            for (var i = 0; i < gridSize; i++)
            {
                fallOffMap[i] = new float[gridSize];
            }

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var xv = x / (float)gridSize * 2 - 1;
                    var yv = y / (float)gridSize * 2 - 1;
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
            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var cell = grid[x][y];
                    if (cell.IsWater) continue;
                    var a = new Vector3(x - 0.5f, 0, y + 0.5f);
                    var b = new Vector3(x + 0.5f, 0, y + 0.5f);
                    var c = new Vector3(x - 0.5f, 0, y - 0.5f);
                    var d = new Vector3(x + 0.5f, 0, y - 0.5f);
                    var uvA = new Vector2Int(x / gridSize, y / gridSize);
                    var uvB = new Vector2Int((x + 1) / gridSize, y / gridSize);
                    var uvC = new Vector2Int(x / gridSize, (y + 1) / gridSize);
                    var uvD = new Vector2Int((x + 1) / gridSize, (y + 1) / gridSize);
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

            _terrainMeshFilter.mesh = mesh;
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
            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
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

                    if (x < gridSize - 1)
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

                    if (y < gridSize - 1)
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

            _terrainMeshRenderer.material = terrainMat;
            // thisMeshRenderer.material.mainTexture = texture;
        }

        private void GenerateTrees(IReadOnlyList<Cell[]> grid)
        {
            var noiseMap = new float[gridSize][];
            for (var i = 0; i < gridSize; i++) //noiseMap Init
            {
                noiseMap[i] = new float[gridSize];
            }

            var (xOffset, yOffset) = (Random.Range(-10000, 10000), Random.Range(-10000, 10000));
            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                    noiseMap[x][y] = noiseValue;
                }
            }

            if (_trees[0].activeSelf)
            {
                for (var i = 0; i < gridSize; i++)
                {
                    _trees[i].SetActive(false);
                }
            }


            var treeIndex = -1;
            _notTreeMap.Clear();

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var cell = grid[x][y];
                    if (cell.IsWater) continue;
                    var v = Random.Range(0, treeDensity);
                    if (noiseMap[x][y] < v)
                    {
                        if (treeIndex >= gridSize - 1) break;
                        treeIndex++;

                        var rotation = Random.Range(1, 5);
                        _trees[treeIndex] = StackObjectPool.Get("Tree", new Vector3(x, 0, y),
                            Quaternion.Euler(0, rotation * 90, 0));
                    }
                    else
                    {
                        _notTreeMap.Add(new Vector3(x, 0, y));
                    }
                }
            }
        }


        private async UniTaskVoid GenerateCrystal()
        {
            crystal.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            crystal.SetActive(true);
            var pos = _notTreeMap[_notTreeMap.Count / 2];
            crystal.transform.position = pos;
        }


        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var cell = _grid[x][y];
                    Gizmos.color = cell.IsWater ? Color.blue : Color.green;
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }
    }
}