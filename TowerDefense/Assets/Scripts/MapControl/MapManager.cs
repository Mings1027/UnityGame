using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapManager : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private MeshFilter _meshFilter;

        private MeshRenderer _meshRenderer;

        // private MeshFilter _obstacleMeshFilter;
        // private MeshRenderer _obstacleMeshRenderer;
        private WaveManager _waveManager;

        private MapData _newMapObject;

        private string _connectionString;

        private Transform _newMapTransform;
        private Vector3 _dirToWayPoint;

        private Vector3[] _checkDirection;
        private Vector3[] _diagonalDir;
        private List<GameObject> _map;
        private List<Vector3> _newMapWayPoints;
        private List<MeshFilter> _meshFilters;
        // private List<MeshFilter> _obstacleMeshFilters;

        private HashSet<Vector3> _expandBtnPosHashSet;

        private List<ExpandMapButton> _expandButtons;

        private MapData[] _neighborMapArray;

        private bool[] _isNullMapArray;
        private bool[] _isConnectedArray;
        private bool _isEndLess;

        private Dictionary<string, string> _directionMappingDic;
        private Dictionary<string, GameObject> _mapDictionary;

        private HashSet<Vector3> _wayPointsHashSet;

        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private NavMeshSurface bossNavMeshSurface;
        [SerializeField] private byte mapSize;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject portalGate;

        [SerializeField] private GameObject[] obstaclePrefabs;

        [SerializeField] private LayerMask groundLayer;

        [SerializeField, Range(0, 100)] private int connectionProbability;
        [SerializeField, Range(0, 200)] private byte maxSize;
        [SerializeField] private Transform mapMesh;
        [SerializeField] private Transform obstacleMesh;

        [SerializeField] private byte maxMapCount;
#if UNITY_EDITOR
        [SerializeField] private bool drawGizmos;
        [SerializeField] private float drawSphereRadius;
        [SerializeField] private Vector3 colliderCenter;
        [SerializeField] private Vector3 colliderSize;
#endif

        #region Unity Event

        private void Awake()
        {
            ComponentInit();
            MapDataInit();
            transform.GetChild(2).gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            _waveManager.OnBossWaveEvent += bossNavMeshSurface.BuildNavMesh;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (_wayPointsHashSet == null) return;
            Gizmos.color = Color.red;
            foreach (var way in _wayPointsHashSet)
            {
                Gizmos.DrawSphere(way, drawSphereRadius);
            }
        }

#endif

        #endregion

        public void MakeMap(int index)
        {
            transform.GetChild(2).gameObject.SetActive(true);
            _waveManager.OnPlaceExpandButtonEvent += PlaceExpandButtons;
            PlaceStartMap(index);
            CombineMesh();
            // CombineObstacleMesh();

            InitExpandButtonPosition();
            PlaceExpandButtons();
            // GenerateAutoMap().Forget();
        }

        #region Init

        private void ComponentInit()
        {
            _waveManager = FindObjectOfType<WaveManager>();
            _meshFilter = mapMesh.GetComponent<MeshFilter>();
            _meshRenderer = mapMesh.GetComponent<MeshRenderer>();
            // _obstacleMeshFilter = obstacleMesh.GetComponent<MeshFilter>();
            // _obstacleMeshRenderer = obstacleMesh.GetComponent<MeshRenderer>();
        }

        private void MapDataInit()
        {
            _checkDirection = new[]
            {
                Vector3.back * mapSize, Vector3.forward * mapSize, Vector3.left * mapSize,
                Vector3.right * mapSize
            };
            _diagonalDir = new[]
            {
                new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, -1), new Vector3(1, 0, -1)
            };
            _map = new List<GameObject>(maxMapCount);
            _newMapWayPoints = new List<Vector3>(4);
            _meshFilters = new List<MeshFilter>(maxMapCount);
            // _obstacleMeshFilters = new List<MeshFilter>(300);
            _expandBtnPosHashSet = new HashSet<Vector3>(100);
            _expandButtons = new List<ExpandMapButton>(100);

            _neighborMapArray = new MapData[4];
            _isNullMapArray = new bool[4];
            _isConnectedArray = new bool[4];

            _directionMappingDic = new Dictionary<string, string>
            {
                { "0", "P" }, { "1", "P" }, { "2", "P" }, { "3", "P" },
                { "01", "S" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "S" },
                { "012", "SL" }, { "013", "SR" }, { "023", "LR" }, { "123", "LR" },
                { "0123", "SLR" }
            };

            _mapDictionary = new Dictionary<string, GameObject>();
            for (var i = 0; i < mapPrefabs.Length; i++)
            {
                var mapName = mapPrefabs[i].name.Split('_')[0];
                _mapDictionary.Add(mapName, mapPrefabs[i]);
            }

            _wayPointsHashSet = new HashSet<Vector3>();
        }

        private void PlaceStartMap(int index)
        {
            var startMapIndex = SortMapIndex();
            for (var i = 0; i < index; i++)
            {
                _connectionString += startMapIndex[i];
            }

            _connectionString = string.Concat(_connectionString.OrderBy(c => c));

            if (_connectionString != null)
                _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], mapMesh)
                    .GetComponent<MapData>();
            _newMapTransform = _newMapObject.transform;
            SetNewMapForward();
            PlaceObstacle();
            _map.Add(_newMapObject.gameObject);
            navMeshSurface.BuildNavMesh();
        }

        private string SortMapIndex()
        {
            const string mapIndex = "0123";
            var charArray = mapIndex.ToCharArray();
            var random = new System.Random();

            var charArrayLength = charArray.Length - 1;
            for (var i = charArrayLength; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                (charArray[i], charArray[j]) = (charArray[j], charArray[i]);
            }

            return new string(charArray);
        }

        private void InitExpandButtonPosition()
        {
            var connectionLength = _connectionString.Length;
            for (var i = 0; i < connectionLength; i++)
            {
                var indexChar = _connectionString[i];
                var index = int.Parse(indexChar.ToString());
                _expandBtnPosHashSet.Add(_checkDirection[index]);
            }
        }

        #endregion

        private void ExpandMap(Transform newMapPos)
        {
            _newMapTransform = newMapPos;
            DisableExpandButtons();
            InitConnectionState();
            CheckNeighborMap();
            CheckConnectedDirection();
            AddRandomDirection();
            PlaceNewMap();
            SetNewMapForward();
            IsPortalMap();
            RemoveWayPoints();
            SetWayPoints();
            PlaceObstacle();
            SetMap().Forget();
        }

        #region ExpandMap Function

        private void DisableExpandButtons()
        {
            var expandBtnCount = _expandButtons.Count;
            for (var i = 0; i < expandBtnCount; i++)
            {
                if (_expandButtons[i].gameObject.activeSelf)
                {
                    _expandButtons[i].gameObject.SetActive(false);
                }
            }

            _expandButtons.Clear();
        }

        private void InitConnectionState()
        {
            var neighborMapCount = _neighborMapArray.Length;
            for (var i = 0; i < neighborMapCount; i++)
            {
                _neighborMapArray[i] = null;
                _isNullMapArray[i] = false;
                _isConnectedArray[i] = false;
            }
        }

        private void CheckNeighborMap()
        {
            var checkDirCount = _checkDirection.Length;
            for (var i = 0; i < checkDirCount; i++)
            {
                var ray = new Ray(_newMapTransform.position, _checkDirection[i]);
                if (Physics.SphereCast(ray, 2, out var hit, mapSize, groundLayer))
                {
                    if (hit.collider.TryGetComponent(out MapData mapData))
                    {
                        _neighborMapArray[i] = mapData;
                    }
                }
                else
                {
                    _isNullMapArray[i] = true;
                }
            }
        }

        private void CheckConnectedDirection()
        {
            var neighborMapCount = _neighborMapArray.Length;
            for (var i = 0; i < neighborMapCount; i++)
            {
                if (_isNullMapArray[i]) continue;

                var neighborPos = _neighborMapArray[i].transform.position;
                var neighborToNewMapDir = (_newMapTransform.position - neighborPos).normalized;
                var neighborWayPoints = _neighborMapArray[i].wayPointList;
                var neighborWayPointCount = neighborWayPoints.Count;
                for (var j = 0; j < neighborWayPointCount; j++)
                {
                    var dir = (neighborWayPoints[j] - neighborPos).normalized;

                    if (neighborToNewMapDir != dir) continue;
                    _isConnectedArray[i] = true;
                }
            }
        }

        private void AddRandomDirection()
        {
            _connectionString = null;
            var tempProbability = connectionProbability;
            var nullMapArrayLength = _isNullMapArray.Length;
            for (var i = 0; i < nullMapArrayLength; i++)
            {
                if (_isNullMapArray[i])
                {
                    var ran = Random.Range(0, 101);
                    if (ran <= tempProbability)
                    {
                        tempProbability -= 10;
                        _connectionString += i;
                    }
                }
                else
                {
                    if (_isConnectedArray[i])
                    {
                        _connectionString += i;
                    }
                }
            }
        }

        private void PlaceNewMap()
        {
            if (_connectionString == null) return;

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]],
                _newMapTransform.position, Quaternion.identity, mapMesh).GetComponent<MapData>();

            _map.Add(_newMapObject.gameObject);
        }

        private void SetNewMapForward()
        {
            var firstIndex = int.Parse(_connectionString[0].ToString());
            _newMapTransform.forward = -_checkDirection[firstIndex];
            _newMapObject.transform.forward = _newMapTransform.forward;
            _newMapObject.SetWayPoint(mapSize / 2);
        }

        private void IsPortalMap()
        {
            if (_connectionString.Length != 1) return;
            var t = _newMapObject.transform;
            var forward = t.forward;
            Instantiate(portalGate, t.position + forward * 1.75f,
                Quaternion.identity, mapMesh).transform.forward = -forward;
        }

        private void RemoveWayPoints()
        {
            _newMapWayPoints.Clear();
            var mapWayPointCount = _newMapObject.wayPointList.Count;
            for (var i = 0; i < mapWayPointCount; i++)
            {
                _newMapWayPoints.Add(_newMapObject.wayPointList[i]);
            }

            var newMapWayPointCount = _newMapWayPoints.Count;
            for (var i = 0; i < newMapWayPointCount; i++)
            {
                _wayPointsHashSet.RemoveWhere(p => p == _newMapWayPoints[i]);
            }

            _expandBtnPosHashSet.RemoveWhere(p => p == _newMapTransform.position);
        }

        private void SetWayPoints()
        {
            //For Portal Map
            if (_newMapWayPoints.Count == 1)
            {
                var t = _newMapObject.transform;
                _wayPointsHashSet.Add(t.position + t.forward * 1.75f);
                return;
            }

            var newMapWayPointCount = _newMapWayPoints.Count;
            for (var i = 0; i < newMapWayPointCount; i++)
            {
                if (!CanAddWayPoints(_newMapWayPoints[i])) continue;
                _wayPointsHashSet.Add(_newMapTransform.position + _dirToWayPoint * mapSize * 0.5f);
                if (CheckLimitMap(_newMapWayPoints[i]))
                {
                    _expandBtnPosHashSet.Add(_newMapTransform.position + _dirToWayPoint * mapSize);
                }
            }
        }

        private void PlaceObstacle()
        {
            var count = Random.Range(0, _newMapObject.placementTile.Count);
            for (var i = 0; i < count; i++)
            {
                var ranIndex = Random.Range(0, _newMapObject.placementTile.Count);
                RandomObstacle(_newMapObject.placementTile[ranIndex]);
                _newMapObject.placementTile.RemoveAt(ranIndex);
            }
        }

        private void RandomObstacle(Vector3 center)
        {
            var pos = center + _diagonalDir[Random.Range(0, _diagonalDir.Length)];
            var ranObstacle = Random.Range(0, obstaclePrefabs.Length);
            Instantiate(obstaclePrefabs[ranObstacle], pos, Quaternion.Euler(0, Random.Range(0, 360), 0),
                obstacleMesh);
        }

        private async UniTaskVoid SetMap()
        {
            await _newMapObject.transform.DOScale(1, 0.25f).From(0).WithCancellation(_cts.Token);
            CombineMesh();
            // CombineObstacleMesh();
            navMeshSurface.BuildNavMesh();
            _waveManager.WaveInit();
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            var wayPoints = _wayPointsHashSet.ToArray();
            _waveManager.WaveStart(wayPoints, _expandBtnPosHashSet.Count == 0);
        }

        //Call When Wave is over
        private void PlaceExpandButtons()
        {
            foreach (var pos in _expandBtnPosHashSet)
            {
                _expandButtons.Add(PoolObjectManager.Get<ExpandMapButton>(PoolObjectKey.ExpandButton, pos));
            }

            var expandBtnCount = _expandButtons.Count;
            for (var i = 0; i < expandBtnCount; i++) _expandButtons[i].OnExpandMapEvent += ExpandMap;
        }

        private void CombineMesh()
        {
            _meshFilters.Clear();
            var mapCount = _map.Count;
            for (var i = 0; i < mapCount; i++)
            {
                if (_map[i].transform.GetChild(0).TryGetComponent(out MeshFilter m))
                {
                    _meshFilters.Add(m);
                }
            }

            var combineInstance = new CombineInstance[_meshFilters.Count];

            for (var i = 0; i < _meshFilters.Count; i++)
            {
                combineInstance[i].mesh = _meshFilters[i].sharedMesh;
                combineInstance[i].transform = _meshFilters[i].transform.localToWorldMatrix;
            }

            var mesh = _meshFilter.mesh;
            mesh.Clear();
            mesh.CombineMeshes(combineInstance);
            if (_meshFilters[0].TryGetComponent(out MeshRenderer r))
            {
                _meshRenderer.sharedMaterial = r.sharedMaterial;
            }

            for (var i = 0; i < _meshFilters.Count; i++)
            {
                if (_meshFilters[i].TryGetComponent(out MeshRenderer meshRenderer))
                {
                    Destroy(meshRenderer);
                }
            }
        }

        // private void CombineObstacleMesh()
        // {
        //     _obstacleMeshFilters.Clear();
        //     for (var i = 0; i < obstacleMesh.childCount; i++)
        //     {
        //         if (obstacleMesh.GetChild(i).GetChild(0).TryGetComponent(out MeshFilter m))
        //         {
        //             _obstacleMeshFilters.Add(m);
        //         }
        //     }
        //
        //     if (_obstacleMeshFilters.Count <= 0) return;
        //
        //     var combineInstance = new CombineInstance[_obstacleMeshFilters.Count];
        //     for (var i = 0; i < _obstacleMeshFilters.Count; i++)
        //     {
        //         combineInstance[i].mesh = _obstacleMeshFilters[i].sharedMesh;
        //         combineInstance[i].transform = _obstacleMeshFilters[i].transform.localToWorldMatrix;
        //     }
        //
        //     var mesh = _obstacleMeshFilter.mesh;
        //     mesh.Clear();
        //     mesh.CombineMeshes(combineInstance);
        //     if (_obstacleMeshFilters[0].TryGetComponent(out MeshRenderer r))
        //     {
        //         _obstacleMeshRenderer.sharedMaterial = r.sharedMaterial;
        //     }
        //
        //     for (var i = 0; i < _obstacleMeshFilters.Count; i++)
        //     {
        //         if (_obstacleMeshFilters[i].TryGetComponent(out MeshRenderer meshRenderer))
        //             Destroy(meshRenderer);
        //     }
        // }

        // You can add wayPoints where no ground.
        private bool CanAddWayPoints(Vector3 newWayPoint)
        {
            _newMapTransform = _newMapObject.transform;
            _dirToWayPoint = (newWayPoint - _newMapTransform.position).normalized;
            if (_dirToWayPoint == Vector3.zero) return false;
            var ray = new Ray(_newMapTransform.position, _dirToWayPoint);
            return !Physics.SphereCast(ray, 2, mapSize, groundLayer);
        }

        // You can add newMap in maxSize
        private bool CheckLimitMap(Vector3 newWayPoint)
        {
            return newWayPoint.x >= -maxSize && newWayPoint.x <= maxSize &&
                   newWayPoint.z >= -maxSize && newWayPoint.z <= maxSize;
        }

        #endregion

#if UNITY_EDITOR
        //  When test random map
        private async UniTaskVoid GenerateAutoMap()
        {
            while (maxMapCount > _map.Count)
            {
                var index = 0;
                for (var i = 0; i < _expandButtons.Count; i++)
                {
                    if (!_expandButtons[i].gameObject.activeSelf) continue;
                    var ran = Random.Range(0, 2);
                    if (ran != 1) continue;
                    index = i;
                    break;
                }

                _expandButtons[index].Expand();
                await UniTask.Yield();
                PlaceExpandButtons();
            }
        }

        [ContextMenu("Set Map Collider")]
        private void SetMapCollider()
        {
            for (int i = 0; i < mapPrefabs.Length; i++)
            {
                mapPrefabs[i].GetComponent<BoxCollider>().center = colliderCenter;
                mapPrefabs[i].GetComponent<BoxCollider>().size = colliderSize;
            }
        }
#endif
    }
}