using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using DataControl;
using ManagerControl;
using PoolObjectControl;
using UIControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapManager : MonoBehaviour
    {
        private WaveManager _waveManager;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshFilter _obstacleMeshFilter;
        private MeshRenderer _obstacleMeshRenderer;

        private GameObject _newMapObject;

        // private string _connectionString;
        // private string _nullMapIndexString;

        private StringBuilder _connectionSb;
        private StringBuilder _nullMapIndexSb;

        private Vector3 _newMapPosition;
        private Vector3 _newMapForward;
        private Vector3 _dirToWayPoint;

        private Vector3[] _checkDirection;
        private List<GameObject> _map;
        private List<Vector3> _newMapWayPoints;
        private List<MeshFilter> _meshFilters;
        private List<MeshFilter> _obstacleMeshFilters;

        private HashSet<Vector3> _expandButtonPosHashSet;
        private List<Vector3> _expandButtonPosList;

        private List<ExpandMapButton> _expandButtons;

        private MapData[] _neighborMapArray;

        private bool[] _isNullMapArray;
        private bool[] _isConnectedArray;

        private Dictionary<string, string> _directionMappingDic;
        private Dictionary<string, GameObject> _mapDictionary;

        private HashSet<Vector3> _wayPointsHashSet;

        [SerializeField] private int mapSize;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject[] uniqueMap;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField, Range(0, 100)] private int portalSpawnProbability;
        [SerializeField] private int maxSize;
        [SerializeField] private Transform obstacleMesh;
        [SerializeField] private int mapCount;
#if UNITY_EDITOR
        [SerializeField] private bool drawGizmos;
#endif

        #region Unity Event

        private void Awake()
        {
            _waveManager = FindObjectOfType<WaveManager>();
            ComponentInit();
            MapDataInit();
        }

        private void Start()
        {
            PlaceStartMap();

            _waveManager.OnPlaceExpandButtonEvent += PlaceExpandButtons;

            TowerManager.Instance.transform.GetComponentInChildren<MainMenuUIController>().OnGenerateInitMapEvent +=
                () => GenerateAutoMap();
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (_wayPointsHashSet == null) return;
            Gizmos.color = Color.red;
            foreach (var way in _wayPointsHashSet)
            {
                Gizmos.DrawSphere(way, 1);
            }
        }

#endif

        #endregion

        #region Init

        private void ComponentInit()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _obstacleMeshFilter = obstacleMesh.GetComponent<MeshFilter>();
            _obstacleMeshRenderer = obstacleMesh.GetComponent<MeshRenderer>();
        }

        private void MapDataInit()
        {
            _connectionSb = new StringBuilder();
            _nullMapIndexSb = new StringBuilder();

            _checkDirection = new[]
            {
                Vector3.back * mapSize, Vector3.forward * mapSize, Vector3.left * mapSize,
                Vector3.right * mapSize
            };
            _map = new List<GameObject>(mapCount);
            _newMapWayPoints = new List<Vector3>(4);
            _meshFilters = new List<MeshFilter>(150);
            _obstacleMeshFilters = new List<MeshFilter>(150);

            _expandButtonPosHashSet = new HashSet<Vector3>(100);
            _expandButtonPosList = new List<Vector3>(100);
            _expandButtons = new List<ExpandMapButton>(100);

            _neighborMapArray = new MapData[4];
            _isNullMapArray = new bool[4];
            _isConnectedArray = new bool[4];


            _directionMappingDic = new Dictionary<string, string>
            {
                { "01", "S" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "S" },
                { "10", "S" }, { "20", "L" }, { "30", "R" }, { "21", "R" }, { "31", "L" }, { "32", "S" },
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

        private void PlaceStartMap()
        {
            var ranIndex = Random.Range(0, _directionMappingDic.Count);
            _connectionSb.Append(_directionMappingDic.Keys.ToArray()[ranIndex]);
            // _connectionString = _directionMappingDic.Keys.ToArray()[ranIndex];

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionSb.ToString()]], transform);

            _newMapObject.TryGetComponent(out MapData mapData);
            SetNewMapForward(mapData);
            PlaceObstacle(mapData);
            _map.Add(_newMapObject);
        }

        private void GenerateInitMap()
        {
            InitExpandButtonPosition();
            PlaceExpandButtons();
        }

        private void InitExpandButtonPosition()
        {
            for (int i = 0; i < _connectionSb.Length; i++)
            {
                var index = int.Parse(_connectionSb[i].ToString());
                _expandButtonPosHashSet.Add(_checkDirection[index]);
            }
            // foreach (var indexChar in _connectionString)
            // {
            //     var index = int.Parse(indexChar.ToString());
            //     _expandButtonPosHashSet.Add(_checkDirection[index]);
            // }
        }

        #endregion

        private void ExpandMap(Vector3 newMapPos)
        {
            InitAdjacentState();

            CheckNeighborMap(newMapPos);

            CheckConnectedDirection(newMapPos);

            RandomConnection();

            PlaceNewMap(newMapPos);

            PoolObjectManager.Get(PoolObjectKey.ExpandMapSmoke, newMapPos);
            _newMapObject.TryGetComponent(out MapData mapData);

            SetNewMapForward(mapData);

            RemovePoints(mapData, newMapPos);

            SetPoints();

            DisableExpandButtons();

            PlaceObstacle(mapData);

            CombineMesh();

            CombineObstacleMesh();
            // _waveManager.enabled = true;
            // _waveManager.StartWave(_wayPointsHashSet.ToList());
            // TowerManager.Instance.EnableTower();
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

        private void RandomConnection()
        {
            _connectionSb.Clear();
            _nullMapIndexSb.Clear();
            // _connectionString = null;
            // _nullMapIndexString = null;

            var count = 0;
            for (var i = 0; i < _isNullMapArray.Length; i++)
            {
                if (_isNullMapArray[i])
                {
                    count++;
                    _nullMapIndexSb.Append(i);
                    // _nullMapIndexString += i;
                    var ran = Random.Range(0, 100);
                    if (ran <= 90 - count * 20)
                    {
                        _connectionSb.Append(i);
                        // _connectionString += i;
                    }
                }
                else
                {
                    if (_isConnectedArray[i])
                    {
                        _connectionSb.Append(i);
                        // _connectionString += i;
                    }
                }
            }
        }

        private void PlaceNewMap(Vector3 newMapPos)
        {
            // if (_connectionString == null) return;
            // var prefabInstantiate = _connectionString.Length == 1 ? IfSingleConnection() : IfMultipleConnection();

            if (_connectionSb.Length.Equals(0)) return;
            var prefabInstantiate = _connectionSb.Length.Equals(1) ? IfSingleConnection() : IfMultipleConnection();
            _newMapObject = Instantiate(prefabInstantiate, newMapPos, Quaternion.identity, transform);
            _map.Add(_newMapObject);
        }

        private void SetNewMapForward(MapData map)
        {
            // var firstIndex = int.Parse(_connectionString[0].ToString());
            var firstIndex = int.Parse(_connectionSb[0].ToString());
            var a = _connectionSb[0];
            _newMapForward = -_checkDirection[firstIndex];
            _newMapObject.transform.forward = _newMapForward;
            map.SetWayPoint(mapSize / 2);
        }

        private void PlaceObstacle(MapData map)
        {
            var count = Random.Range(0, map.placementTile.Count);
            for (int i = 0; i < count; i++)
            {
                var ranIndex = Random.Range(0, map.placementTile.Count);
                RandomPlaceObstacle(map, map.placementTile[ranIndex]);
                map.placementTile.RemoveAt(ranIndex);
            }
        }

        private void RandomPlaceObstacle(MapData map, Vector3 center)
        {
            var diagonalCount = Random.Range(0, map.diagonalDir.Count);
            for (int i = 0; i < diagonalCount; i++)
            {
                var ranIndex = Random.Range(0, map.diagonalDir.Count);
                var pos = center + map.diagonalDir[ranIndex];
                map.diagonalDir.RemoveAt(ranIndex);

                var ranObstacle = Random.Range(0, obstaclePrefabs.Length);
                Instantiate(obstaclePrefabs[ranObstacle], pos, Quaternion.Euler(0, Random.Range(0, 360), 0),
                    obstacleMesh);
            }
        }

        private GameObject IfSingleConnection()
        {
            // if (_nullMapIndexString == null || CanSpawnPortal())
            // {
            //     return uniqueMap[0];
            // }
            //
            // _connectionString += _nullMapIndexString[Random.Range(0, _nullMapIndexString.Length)];
            // // _connectionString = string.Concat(_connectionString.OrderBy(c => c));
            //
            // return _mapDictionary[_directionMappingDic[_connectionString]];
            if (_nullMapIndexSb.Length.Equals(0) || CanSpawnPortal())
            {
                return uniqueMap[0];
            }

            _connectionSb.Append(_nullMapIndexSb[Random.Range(0, _nullMapIndexSb.Length)]);
            return _mapDictionary[_directionMappingDic[_connectionSb.ToString()]];
        }

        private GameObject IfMultipleConnection()
        {
            return _mapDictionary[_directionMappingDic[_connectionSb.ToString()]];
            // return _mapDictionary[_directionMappingDic[_connectionString]];
        }

        private bool CanSpawnPortal()
        {
            var ranProbability = Random.Range(0, 100);
            return ranProbability <= portalSpawnProbability;
        }

        private void RemovePoints(MapData mapData, Vector3 newMapPos)
        {
            _newMapWayPoints.Clear();
            for (var i = 0; i < mapData.wayPointList.Count; i++)
            {
                _newMapWayPoints.Add(mapData.wayPointList[i]);
            }

            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                _wayPointsHashSet.RemoveWhere(p => p == _newMapWayPoints[i]);
            }

            _expandButtonPosHashSet.RemoveWhere(p => p == newMapPos);
        }

        private void SetPoints()
        {
            //For Portal Map
            if (_newMapWayPoints.Count == 1)
            {
                _wayPointsHashSet.Add(_newMapObject.transform.position + _newMapObject.transform.forward * 4);
                return;
            }

            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                if (!CanAddWayPoints(_newMapWayPoints[i])) continue;
                _wayPointsHashSet.Add(_newMapPosition + _dirToWayPoint * mapSize * 0.5f);
                if (CheckLimitMap(_newMapWayPoints[i]))
                {
                    _expandButtonPosHashSet.Add(_newMapPosition + _dirToWayPoint * mapSize);
                }
            }
#if UNITY_EDITOR
            if (_wayPointsHashSet.Count == 0)
                print("00000000000000000");
#endif
        }

        private void DisableExpandButtons()
        {
            for (var i = 0; i < _expandButtons.Count; i++)
            {
                if (_expandButtons[i].gameObject.activeSelf)
                {
                    _expandButtons[i].gameObject.SetActive(false);
                }
            }

            _expandButtons.Clear();
        }

        //Call When Wave is over
        private void PlaceExpandButtons()
        {
            SoundManager.Instance.PlayBGM(StringManager.WaveBreak);
            _expandButtonPosList.Clear();
            _expandButtonPosList = _expandButtonPosHashSet.ToList();

            for (var i = 0; i < _expandButtonPosList.Count; i++)
            {
                _expandButtons.Add(
                    PoolObjectManager.Get<ExpandMapButton>(PoolObjectKey.ExpandButton, _expandButtonPosList[i]));
                _expandButtons[i].OnExpandMapEvent += ExpandMap;
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

        private void CombineObstacleMesh()
        {
            _obstacleMeshFilters.Clear();
            for (var i = 0; i < obstacleMesh.childCount; i++)
            {
                if (obstacleMesh.GetChild(i).GetChild(0).TryGetComponent(out MeshFilter m))
                {
                    _obstacleMeshFilters.Add(m);
                }
            }

            if (_obstacleMeshFilters.Count <= 0) return;

            var combineInstance = new CombineInstance[_obstacleMeshFilters.Count];
            for (var i = 0; i < _obstacleMeshFilters.Count; i++)
            {
                combineInstance[i].mesh = _obstacleMeshFilters[i].sharedMesh;
                combineInstance[i].transform = _obstacleMeshFilters[i].transform.localToWorldMatrix;
            }

            var mesh = _obstacleMeshFilter.mesh;
            mesh.Clear();
            mesh.CombineMeshes(combineInstance);
            if (_obstacleMeshFilters[0].TryGetComponent(out MeshRenderer r))
            {
                _obstacleMeshRenderer.sharedMaterial = r.sharedMaterial;
            }

            for (var i = 0; i < _obstacleMeshFilters.Count; i++)
            {
                if (_obstacleMeshFilters[i].TryGetComponent(out MeshRenderer meshRenderer))
                    Destroy(meshRenderer);
            }
        }

        // You can add wayPoints where no ground.
        private bool CanAddWayPoints(Vector3 newWayPoint)
        {
            _newMapPosition = _newMapObject.transform.position;
            _dirToWayPoint = (newWayPoint - _newMapPosition).normalized;
            if (_dirToWayPoint == Vector3.zero) return false;
            var ray = new Ray(_newMapPosition, _dirToWayPoint);
            return !Physics.SphereCast(ray, 2, mapSize, groundLayer);
        }

        // You can add newMap in maxSize
        private bool CheckLimitMap(Vector3 newWayPoint)
        {
            return newWayPoint.x >= -maxSize && newWayPoint.x <= maxSize &&
                   newWayPoint.z >= -maxSize && newWayPoint.z <= maxSize;
        }

#if UNITY_EDITOR
        //  When test random map
        private async UniTaskVoid GenerateAutoMap()
        {
            InitExpandButtonPosition();
            PlaceExpandButtons();

            while (mapCount > _map.Count)
            {
                var index = 0;
                for (int i = 0; i < _expandButtons.Count; i++)
                {
                    if (_expandButtons[i].gameObject.activeSelf)
                    {
                        var ran = Random.Range(0, 2);
                        if (ran == 1)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                _expandButtons[index].Expand();
                await UniTask.Yield();
                PlaceExpandButtons();
            }
        }
#endif
    }
}