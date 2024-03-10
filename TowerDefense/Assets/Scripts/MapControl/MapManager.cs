using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceControl;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapManager : MonoBehaviour, IAddressableObject
    {
        private MeshFilter _meshFilter;
        // private MeshRenderer _meshRenderer;

        // private MeshFilter _obstacleMeshFilter;
        // private MeshRenderer _obstacleMeshRenderer;
        private WaveManager _waveManager;

        private MapData _newMapObject;
        private GameObject _portalGateObject;

        private string _tileConnectionIndex;
        private string _emptyTileIndex;

        private Vector3 _newMapPos;
        private Vector3 _dirToWayPoint;

        private Vector3[] _checkDirection;
        private Vector3[] _diagonalDir;
        private List<GameObject> _map;

        private List<Vector3> _newMapWayPoints;
        private List<MeshFilter> _meshFilters;
        // private List<MeshFilter> _obstacleMeshFilters;

        private HashSet<Vector3> _expandBtnPosHashSet;

        private List<ExpandMapButton> _expandButtons;
        // private List<GameObject> _wayPointPortals;

        private MapData[] _neighborMapArray;

        private bool[] _isEmptyMapArray;
        private bool[] _isConnectedArray;
        private bool _isEndLess;

        private Dictionary<string, string> _directionMappingDic;
        private Dictionary<string, GameObject> _mapDictionary;

        private HashSet<Vector3> _wayPointsHashSet;
        private byte _connectionProbability;

        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private NavMeshSurface bossNavMeshSurface;
        [SerializeField] private NavMeshSurface ramNavMeshSurface;
        [SerializeField] private byte mapSize;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject portalGate;

        [SerializeField] private GameObject[] obstaclePrefabs;

        [SerializeField] private LayerMask groundLayer;

        [SerializeField, Range(0, 200)] private byte mapBoundSize;
        [SerializeField] private Transform mapMesh;
        [SerializeField] private Transform obstacleMesh;

#if UNITY_EDITOR
        private CancellationTokenSource _cts;
        private Queue<string> _customMapQueue;

        [Header("==============For Test==============")]
        [SerializeField] private bool useCustomMap;

        [SerializeField] private string customMap;
        [SerializeField] private byte maxMapCount;
        [SerializeField] private bool drawGizmos;
        [SerializeField] private float drawSphereRadius;
        [SerializeField] private Vector3 colliderCenter;
        [SerializeField] private Vector3 colliderSize;
        [SerializeField] private bool generateAutoMap;
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Gizmos.DrawWireCube(transform.position, new Vector3(mapBoundSize * 2, 1, mapBoundSize * 2));

            if (_wayPointsHashSet == null) return;
            Gizmos.color = Color.red;

            foreach (var way in _wayPointsHashSet)
            {
                Gizmos.DrawSphere(way, drawSphereRadius);
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
#endif

#region Init

        public void Init()
        {
            ComponentInit();
            MapDataInit();

            _waveManager.OnBossWaveEvent += bossNavMeshSurface.BuildNavMesh;
            transform.GetChild(2).gameObject.SetActive(false);
#if UNITY_EDITOR
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
#endif
        }

        private void ComponentInit()
        {
            _waveManager = FindObjectOfType<WaveManager>();
            _meshFilter = mapMesh.GetComponent<MeshFilter>();
            // _meshRenderer = mapMesh.GetComponent<MeshRenderer>();
            // _obstacleMeshFilter = obstacleMesh.GetComponent<MeshFilter>();
            // _obstacleMeshRenderer = obstacleMesh.GetComponent<MeshRenderer>();
        }

        private void MapDataInit()
        {
            _checkDirection = new[]
            {
                Vector3.back * mapSize, Vector3.forward * mapSize, Vector3.left * mapSize, Vector3.right * mapSize
            };
            _diagonalDir = new[]
            {
                new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, -1), new Vector3(1, 0, -1)
            };
            _map = new List<GameObject>();
            _newMapWayPoints = new List<Vector3>(4);
            _meshFilters = new List<MeshFilter>();
            // _obstacleMeshFilters = new List<MeshFilter>(300);
            _expandBtnPosHashSet = new HashSet<Vector3>();
            _expandButtons = new List<ExpandMapButton>();
            // _wayPointPortals = new List<GameObject>();
#if UNITY_EDITOR
            _customMapQueue = new Queue<string>();
            var splitString = customMap.Split(' ');

            foreach (var str in splitString)
            {
                _customMapQueue.Enqueue(str);
            }
#endif
            _neighborMapArray = new MapData[4];
            _isEmptyMapArray = new bool[4];
            _isConnectedArray = new bool[4];

            _directionMappingDic = new Dictionary<string, string>
            {
                { "0", "P" }, { "1", "P" }, { "2", "P" }, { "3", "P" }, { "01", "S" }, { "02", "L" }, { "03", "R" },
                { "12", "R" }, { "13", "L" }, { "23", "S" }, { "012", "SL" }, { "013", "SR" }, { "023", "LR" },
                { "123", "LR" }, { "0123", "SLR" }
            };

            _mapDictionary = new Dictionary<string, GameObject>();

            for (var i = 0; i < mapPrefabs.Length; i++)
            {
                var mapName = mapPrefabs[i].name.Split('_')[0];
                _mapDictionary.Add(mapName, mapPrefabs[i]);
            }

            _wayPointsHashSet = new HashSet<Vector3>();
        }

        private void PlaceStartMap(byte difficulty)
        {
            var startMapIndex = SortMapIndex();

            for (var i = 0; i < difficulty; i++)
            {
                _tileConnectionIndex += startMapIndex[i];
            }

            _tileConnectionIndex = string.Concat(_tileConnectionIndex.OrderBy(c => c));

            if (_tileConnectionIndex != null)
                _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_tileConnectionIndex]], mapMesh)
                    .GetComponent<MapData>();
            _newMapPos = _newMapObject.transform.position;
            SetNewMapForward();
            PlaceObstacle();
            _map.Add(_newMapObject.gameObject);
            navMeshSurface.BuildNavMesh();
            ramNavMeshSurface.BuildNavMesh();
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
            var connectionLength = _tileConnectionIndex.Length;

            for (var i = 0; i < connectionLength; i++)
            {
                var indexChar = _tileConnectionIndex[i];
                var index = int.Parse(indexChar.ToString());
                _expandBtnPosHashSet.Add(_checkDirection[index]);
            }
        }

#endregion

        public void MakeMap(byte difficultyLevel)
        {
            _connectionProbability = difficultyLevel switch
            {
                0 => 25,
                1 => 50,
                2 => 60,
                3 => 70,
                _ => _connectionProbability
            };

            transform.GetChild(2).gameObject.SetActive(true);
            _waveManager.OnPlaceExpandButtonEvent += PlaceExpandButtons;
            _waveManager.enabled = false;
            PlaceStartMap((byte)(difficultyLevel + 1));
            // CombineMesh();
            // CombineObstacleMesh();

            InitExpandButtonPosition();
            PlaceExpandButtons();
#if UNITY_EDITOR
            if (generateAutoMap) GenerateAutoMap().Forget();
#endif
        }

        private void ExpandMap(Vector3 newMapPos)
        {
            _newMapPos = newMapPos;
            DisableExpandButtons();

            InitConnectionState();
            CheckNeighborMap();
#if UNITY_EDITOR
            if (!useCustomMap)
            {
#endif
                CheckConnectedDirection();
                AddRandomDirection();
                PlaceNewMap();
                SetNewMapForward();
                SpawnPortal();
#if UNITY_EDITOR
            }
            else
            {
                PlaceCustomMap();
            }
#endif
            RemoveWayPoints();
            SetWayPoints();
            PlaceObstacle();
#if UNITY_EDITOR
            if (generateAutoMap) return;
#endif
            SetMap();
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
                _isEmptyMapArray[i] = false;
                _isConnectedArray[i] = false;
            }
        }

        private void CheckNeighborMap()
        {
            var checkDirCount = _checkDirection.Length;

            for (var i = 0; i < checkDirCount; i++)
            {
                var ray = new Ray(_newMapPos, _checkDirection[i]);

                if (Physics.SphereCast(ray, 2, out var hit, mapSize, groundLayer))
                {
                    if (hit.collider.TryGetComponent(out MapData mapData))
                    {
                        _neighborMapArray[i] = mapData;
                    }
                }
                else
                {
                    _isEmptyMapArray[i] = true;
                }
            }
        }

        private void CheckConnectedDirection()
        {
            var neighborMapCount = _neighborMapArray.Length;

            for (var i = 0; i < neighborMapCount; i++)
            {
                if (_isEmptyMapArray[i]) continue;

                var neighborPos = _neighborMapArray[i].transform.position;
                var neighborToNewMapDir = (_newMapPos - neighborPos).normalized;
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
            _tileConnectionIndex = null;
            _emptyTileIndex = null;
            var emptyMapArrayLength = _isEmptyMapArray.Length;

            var isConnected = false;
            var tempProbability = _wayPointsHashSet.Count == 1 ? 100 : _connectionProbability;

            for (var i = 0; i < emptyMapArrayLength; i++)
            {
                if (_isEmptyMapArray[i])
                {
                    _emptyTileIndex += i;
                    var ran = Random.Range(0, 101);

                    if (ran <= tempProbability)
                    {
                        isConnected = true;
                        _tileConnectionIndex += i;
                    }
                }
                else
                {
                    if (_isConnectedArray[i])
                    {
                        _tileConnectionIndex += i;
                    }
                }
            }

            if (_emptyTileIndex == null) return;
            //사방이 막히지 않았을 때 아래를 실행

            // 하나만 연결된 경우 랜덤으로 길 하나를 뚫어줌
            if (_tileConnectionIndex is { Length: 1 })
            {
                var tempString = new StringBuilder(_emptyTileIndex);
                _tileConnectionIndex += tempString[Random.Range(0, tempString.Length)];
            }

            //타일이 없는곳에 연결이 한번도 안되었을때 혹은 연결된 인덱스와과 타일이 없는곳의 인덱스 길이 같으면서 정반대일 때 닫힐 수 있으므로 길 하나를 뚫어줌
            if (!isConnected || _tileConnectionIndex != null && _tileConnectionIndex.Length == _emptyTileIndex.Length)
            {
                var ranIndex = Random.Range(0, _emptyTileIndex.Length);
                _tileConnectionIndex += _emptyTileIndex[ranIndex];
                _tileConnectionIndex = new string(_tileConnectionIndex?.Distinct().ToArray());
            }
        }

        private void PlaceNewMap()
        {
            if (_tileConnectionIndex == null) return;
            _tileConnectionIndex = string.Concat(_tileConnectionIndex.OrderBy(c => c));

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_tileConnectionIndex]], _newMapPos,
                Quaternion.identity, mapMesh).GetComponent<MapData>();

            _map.Add(_newMapObject.gameObject);
        }

        private void SetNewMapForward()
        {
            var firstIndex = int.Parse(_tileConnectionIndex[0].ToString());
            _newMapObject.transform.forward = -_checkDirection[firstIndex];
            _newMapObject.SetWayPoint(mapSize / 2);
        }

        private void SpawnPortal()
        {
            if (_tileConnectionIndex.Length != 1) return;
            var t = _newMapObject.transform;
            var forward = t.forward;
            _portalGateObject = Instantiate(portalGate, t.position + forward * 1.75f, Quaternion.identity, mapMesh);
            _portalGateObject.transform.forward = -forward;
        }
#if UNITY_EDITOR
        private void PlaceCustomMap()
        {
            var curCustomMap = _customMapQueue.Dequeue();
            _newMapObject = Instantiate(_mapDictionary[curCustomMap], _newMapPos, Quaternion.identity, mapMesh)
                .GetComponent<MapData>();

            _map.Add(_newMapObject.gameObject);

            for (var i = 0; i < _neighborMapArray.Length; i++)
            {
                if (_isEmptyMapArray[i]) continue;

                var neighborToNewMapDir = (_newMapPos - _neighborMapArray[i].transform.position).normalized;
                _newMapObject.transform.forward = neighborToNewMapDir;
                _newMapObject.SetWayPoint(mapSize / 2);

                break;
            }

            if (curCustomMap == "P")
            {
                var t = _newMapObject.transform;
                var forward = t.forward;
                _portalGateObject = Instantiate(portalGate, t.position + forward * 1.75f, Quaternion.identity, mapMesh);
                _portalGateObject.transform.forward = -forward;
            }
        }
#endif
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

            _expandBtnPosHashSet.RemoveWhere(p => p == _newMapPos);
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
                _wayPointsHashSet.Add(_newMapPos + _dirToWayPoint * mapSize * 0.5f);

                if (CheckLimitMap(_newMapWayPoints[i]))
                {
                    _expandBtnPosHashSet.Add(_newMapPos + _dirToWayPoint * mapSize);
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
           var obstacle= Instantiate(obstaclePrefabs[ranObstacle], pos, Quaternion.Euler(0, Random.Range(0, 360), 0), obstacleMesh);
           obstacle.transform.DOMoveY(1, 1).From(-5).SetEase(Ease.OutBack);
        }

        private async UniTaskVoid SetMap()
        {
            var mapPos = _newMapObject.transform;
            await _newMapObject.transform.DOMoveY(0, 1).From(new Vector3(mapPos.position.x, -3, mapPos.position.z))
                .SetEase(Ease.OutBack);
            // for (int i = 0; i < _neighborMapArray.Length; i++)
            // {
            //     if (_neighborMapArray[i] == null) continue;
            //     DOTween.Sequence().Append(_neighborMapArray[i].transform.DOMoveY(1f, 0.3f).From(0))
            //         .Append(_neighborMapArray[i].transform.DOMoveY(0, 0.3f).From(1f).SetEase(Ease.OutBack));
            // }

            // CombineMesh();
            // CombineObstacleMesh();
            navMeshSurface.BuildNavMesh();
            ramNavMeshSurface.BuildNavMesh();
            _waveManager.WaveInit(_wayPointsHashSet.ToArray()).Forget();
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
            // if (_meshFilters[0].TryGetComponent(out MeshRenderer r))
            // {
            //     _meshRenderer.sharedMaterial = r.sharedMaterial;
            // }

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
            _dirToWayPoint = (newWayPoint - _newMapPos).normalized;

            if (_dirToWayPoint == Vector3.zero) return false;
            var ray = new Ray(_newMapPos, _dirToWayPoint);

            return !Physics.SphereCast(ray, 2, mapSize, groundLayer);
        }

        // You can add newMap in maxSize
        private bool CheckLimitMap(Vector3 newWayPoint)
        {
            return newWayPoint.x >= -mapBoundSize && newWayPoint.x <= mapBoundSize && newWayPoint.z >= -mapBoundSize &&
                   newWayPoint.z <= mapBoundSize;
        }

#endregion

#if UNITY_EDITOR
        //  When test random map
        private async UniTaskVoid GenerateAutoMap()
        {
            while (maxMapCount > _map.Count)
            {
                await UniTask.Yield();
                var expandPosArray = _expandBtnPosHashSet.ToArray();
                ExpandMap(expandPosArray[Random.Range(0, expandPosArray.Length)]);
            }
        }

        [ContextMenu("Set Map Collider")]
        private void SetMapCollider()
        {
            for (var i = 0; i < mapPrefabs.Length; i++)
            {
                mapPrefabs[i].GetComponent<BoxCollider>().center = colliderCenter;
                mapPrefabs[i].GetComponent<BoxCollider>().size = colliderSize;
            }
        }

        [ContextMenu("Set Map Placement Collider")]
        private void SetMapPlacementCollider()
        {
            for (var i = 0; i < mapPrefabs.Length; i++)
            {
                var placementTiles = mapPrefabs[i].GetComponentsInChildren<BoxCollider>();

                for (var j = 1; j < placementTiles.Length; j++)
                {
                    var placementCenter = placementTiles[j].center;
                    placementCenter.y = colliderCenter.y;
                    placementTiles[j].center = placementCenter;

                    var placementSize = placementTiles[j].size;
                    placementSize.y = colliderSize.y;
                    placementTiles[j].size = placementSize;
                }
            }
        }
#endif
    }
}