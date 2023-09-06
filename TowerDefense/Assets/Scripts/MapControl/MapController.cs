using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using ManagerControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapController : Singleton<MapController>
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private GameObject _newMapObject;

        private string _connectionString;
        private string _nullMapIndexString;

        private Vector3 _newMapPosition;
        private Vector3 _newMapForward;
        private Vector3 _dirToWayPoint;

        private Vector3[] _checkDirection;
        private List<GameObject> _map;
        private List<Vector3> _newMapWayPoints;
        private List<MeshFilter> _meshFilters;

        private HashSet<Vector3> _expandButtonPosHashSet;
        private List<Vector3> _expandButtonPosList;

        private List<ExpandMapButton> _expandButtons;

        private MapData[] _neighborMapArray;

        private bool[] _isNullMapArray;
        private bool[] _isConnectedArray;

        private Dictionary<string, GameObject> _mapDictionary;
        private Dictionary<string, string> _directionMappingDic;

        private HashSet<Vector3> _wayPoints;

        [SerializeField] private int mapSize;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject[] uniqueMap;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField, Range(0, 100)] private int portalSpawnProbability;
        [SerializeField] private int maxMapWidth;
        [SerializeField] private int maxMapHeight;
#if UNITY_EDITOR
        [SerializeField] private int mapCount;
#endif
        private void Awake()
        {
            ComponentInit();

            MapDataInit();
        }

        private void Start()
        {
            PlaceStartMap();
            WaveManager.Instance.OnPlaceExpandButton += PlaceExpandButtons;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var way in _wayPoints)
            {
                Gizmos.DrawSphere(way, 1);
            }
        }
#endif
        private void PlaceStartMap()
        {
            var ranIndex = Random.Range(0, _directionMappingDic.Count);
            _connectionString = _directionMappingDic.Keys.ToArray()[ranIndex];

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);

            SetNewMapForward();
            _map.Add(_newMapObject);
        }

        private void ComponentInit()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void MapDataInit()
        {
            _checkDirection = new[]
            {
                Vector3.back * mapSize, Vector3.forward * mapSize, Vector3.left * mapSize,
                Vector3.right * mapSize
            };
            _map = new List<GameObject>();
            _newMapWayPoints = new List<Vector3>(4);
            _meshFilters = new List<MeshFilter>(150);

            _expandButtonPosHashSet = new HashSet<Vector3>();
            _expandButtonPosList = new List<Vector3>();
            _expandButtons = new List<ExpandMapButton>(50);
            _neighborMapArray = new MapData[4];
            _isNullMapArray = new bool[4];
            _isConnectedArray = new bool[4];

            _mapDictionary = new Dictionary<string, GameObject>();
            for (var i = 0; i < mapPrefabs.Length; i++)
            {
                var mapName = mapPrefabs[i].name.Split('_')[0];
                _mapDictionary.Add(mapName, mapPrefabs[i]);
            }

            _directionMappingDic = new Dictionary<string, string>
            {
                { "01", "F" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "F" },
                { "012", "FL" }, { "013", "FR" }, { "023", "LR" }, { "123", "LR" },
                { "0123", "FLR" }
            };

            _wayPoints = new HashSet<Vector3>();
        }

        public async UniTaskVoid GenerateInitMap()
        {
            InitExpandButtonPosition();
            PlaceExpandButtons();

            // while (mapCount > _map.Count)
            // {
            //     var index = 0;
            //     for (int i = 0; i < _expandButtons.Count; i++)
            //     {
            //         if (_expandButtons[i].gameObject.activeSelf)
            //         {
            //             var ran = Random.Range(0, 2);
            //             if (ran == 1)
            //             {
            //                 index = i;
            //                 break;
            //             }
            //         }
            //     }
            //
            //     _expandButtons[index].ExpandMap();
            //     await UniTask.Yield();
            //     PlaceExpandButtons();
            // }
        }

        private void InitExpandButtonPosition()
        {
            foreach (var indexChar in _connectionString)
            {
                var index = int.Parse(indexChar.ToString());
                _expandButtonPosHashSet.Add(_checkDirection[index]);
            }
        }

        public void ExpandMap(Vector3 newMapPos)
        {
            InitAdjacentState();

            CheckNeighborMap(newMapPos);

            CheckConnectedDirection(newMapPos);

            CheckConnection();

            PlaceNewMap(newMapPos);

            ObjectPoolManager.Get(PoolObjectName.ExpandMapSmoke, newMapPos);

            SetNewMapForward();

            SetWayPoints();

            SetButtonsPosition(newMapPos);

            DisableExpandButtons();

            CombineMesh();

            WaveManager.Instance.StartWave(_wayPoints.ToArray());
        }

        private void InitAdjacentState()
        {
            for (var i = 0; i < _neighborMapArray.Length; i++)
            {
                _neighborMapArray[i] = null;
                _isNullMapArray[i] = false;
                _isConnectedArray[i] = false;
            }
        }

        private void CheckNeighborMap(Vector3 newMapPos)
        {
            for (var i = 0; i < _checkDirection.Length; i++)
            {
                var ray = new Ray(newMapPos, _checkDirection[i]);
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

        private void CheckConnectedDirection(Vector3 newMapPos)
        {
            for (var i = 0; i < _neighborMapArray.Length; i++)
            {
                if (_isNullMapArray[i]) continue;

                var neighborPos = _neighborMapArray[i].transform.position;
                var neighborToNewMapDir = (newMapPos - neighborPos).normalized;
                var neighborWayPoints = _neighborMapArray[i].wayPointList;
                for (var j = 0; j < neighborWayPoints.Count; j++)
                {
                    var dir = (neighborWayPoints[j] - neighborPos).normalized;

                    if (neighborToNewMapDir != dir) continue;
                    _isConnectedArray[i] = true;
                }
            }
        }

        private void CheckConnection()
        {
            _connectionString = null;
            _nullMapIndexString = null;
            for (var i = 0; i < _isNullMapArray.Length; i++)
            {
                if (_isNullMapArray[i])
                {
                    _nullMapIndexString += i;
                    var ran = Random.Range(0, 2);
                    if (ran == 1) _connectionString += i;
                }
                else
                {
                    if (_isConnectedArray[i]) _connectionString += i;
                }
            }
        }

        private void PlaceNewMap(Vector3 newMapPos)
        {
            if (_connectionString == null) return;
            var prefabInstantiate = _connectionString.Length == 1 ? IfSingleConnection() : IfMultipleConnection();

            _newMapObject = Instantiate(prefabInstantiate, newMapPos, Quaternion.identity, transform);
            _map.Add(_newMapObject);
        }

        private void SetNewMapForward()
        {
            var firstIndex = int.Parse(_connectionString[0].ToString());
            _newMapForward = -_checkDirection[firstIndex];
            _newMapObject.transform.forward = _newMapForward;
            _newMapObject.TryGetComponent(out MapData map);
            map.SetWayPoint();
        }

        private GameObject IfSingleConnection()
        {
            if (_nullMapIndexString == null || CanSpawnPortal())
            {
                return uniqueMap[0];
            }

            _connectionString += _nullMapIndexString[Random.Range(0, _nullMapIndexString.Length)];
            _connectionString = string.Concat(_connectionString.OrderBy(c => c));
            return _mapDictionary[_directionMappingDic[_connectionString]];
        }

        private GameObject IfMultipleConnection()
        {
            return _mapDictionary[_directionMappingDic[_connectionString]];
        }

        private bool CanSpawnPortal()
        {
            var ranProbability = Random.Range(0, 100);
            return ranProbability <= portalSpawnProbability;
        }

        private void SetWayPoints()
        {
            _newMapObject.TryGetComponent(out MapData mapData);
            _newMapWayPoints.Clear();
            for (var i = 0; i < mapData.wayPointList.Count; i++)
            {
                _newMapWayPoints.Add(mapData.wayPointList[i]);
            }

            for (var i = 0; i < mapData.wayPointList.Count; i++)
            {
                _wayPoints.RemoveWhere(p => p == mapData.wayPointList[i]);
            }

            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                if (CanAddToList(_newMapWayPoints[i]))
                {
                    _wayPoints.Add(_newMapPosition + _dirToWayPoint * mapSize * 0.5f);
                }
            }

            //For Portal Map
            if (mapData.wayPointList.Count == 1)
            {
                _wayPoints.Add(_newMapObject.transform.position);
            }
        }

        private void SetButtonsPosition(Vector3 newMapPos)
        {
            _expandButtonPosHashSet.RemoveWhere(p => p == newMapPos);

            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                if (CanAddToList(_newMapWayPoints[i]))
                {
                    _expandButtonPosHashSet.Add(_newMapPosition + _dirToWayPoint * mapSize);
                }
            }
        }

        private void DisableExpandButtons()
        {
            for (var i = 0; i < _expandButtonPosList.Count; i++)
            {
                if (_expandButtons[i].gameObject.activeSelf) _expandButtons[i].gameObject.SetActive(false);
            }

            _expandButtons.Clear();
        }

        //Call When Wave is over
        private void PlaceExpandButtons()
        {
            if (_map.Count > mapCount) return;
            _expandButtonPosList.Clear();
            _expandButtonPosList = _expandButtonPosHashSet.ToList();

            for (var i = 0; i < _expandButtonPosList.Count; i++)
            {
                _expandButtons.Add(
                    ObjectPoolManager.Get<ExpandMapButton>(
                        PoolObjectName.ExpandButton, _expandButtonPosList[i], Quaternion.Euler(0, 45, 0)));
            }
        }

        private void CombineMesh()
        {
            _meshFilters.Clear();
            for (var i = 0; i < _map.Count; i++)
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
// #if UNITY_EDITOR
//             const string path = "Assets/Meshes/Map.asset";
//             AssetDatabase.CreateAsset(_meshFilter.mesh, AssetDatabase.GenerateUniqueAssetPath(path));
//             AssetDatabase.SaveAssets();
// #endif
        }

        private bool CanAddToList(Vector3 point)
        {
            _newMapPosition = _newMapObject.transform.position;
            _dirToWayPoint = (point - _newMapPosition).normalized;
            if (_dirToWayPoint == Vector3.zero) return false;
            var ray = new Ray(_newMapPosition, _dirToWayPoint);
            if (Physics.SphereCast(ray, 2, mapSize, groundLayer)) return false;

            return point.x >= -maxMapWidth && point.x <= maxMapWidth &&
                   point.z >= -maxMapHeight && point.z <= maxMapHeight;
        }
    }
}