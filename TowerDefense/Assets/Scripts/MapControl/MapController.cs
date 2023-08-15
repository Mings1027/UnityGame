using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapController : Singleton<MapController>
    {
        [SerializeField] private int saveMeshCount;

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

        private Button[] _expandButtons;

        private MapData[] _neighborMapArray;

        private bool[] _isNullMapArray;
        private bool[] _isConnectedArray;

        private Dictionary<string, GameObject> _mapDictionary;
        private Dictionary<string, string> _directionMappingDic;

        private HashSet<Vector3> _wayPoints;

        [SerializeField] private float mapSize;
        [SerializeField] private Transform expandBtnParent;
        [SerializeField] private GameObject expandBtn;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject[] uniqueMap;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField, Range(0, 100)] private int portalSpawnProbability;
        [SerializeField] private int maxMapWidth;
        [SerializeField] private int maxMapHeight;
        [SerializeField] private int mapCount;

        private void Awake()
        {
            ComponentInit();

            MapDataInit();
        }

        private void ComponentInit()
        {
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            _meshFilter = GetComponent<MeshFilter>();

            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

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
            _expandButtons = new Button[40];
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
                { "01", "S" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "S" },
                { "012", "SL" }, { "013", "SR" }, { "023", "LR" }, { "123", "LR" },
                { "0123", "SLR" }
            };

            _wayPoints = new HashSet<Vector3>();
        }

        private void Start()
        {
            for (var i = 0; i < 40; i++)
            {
                _expandButtons[i] = Instantiate(expandBtn, expandBtnParent).GetComponent<Button>();
                _expandButtons[i].gameObject.SetActive(false);
            }

            PlaceStartMap();
            WaveManager.Instance.OnPlaceExpandBtnEvent += PlaceExpandButtons;
        }

        public void GenerateInitMap()
        {
            InitExpandButtonPosition();
            PlaceExpandButtons();
        }

        private void PlaceStartMap()
        {
            var ranIndex = Random.Range(0, _directionMappingDic.Count);
            _connectionString = _directionMappingDic.Keys.ToArray()[ranIndex];

            var newMap = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);

            var firstIndex = int.Parse(_connectionString[0].ToString());
            _newMapForward = -_checkDirection[firstIndex];
            newMap.transform.forward = _newMapForward;
            _map.Add(newMap);
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

            PlaceNewMap();

            SetNewMapForward(newMapPos);

            SetButtonsPosition(newMapPos);

            SetWayPoints();

            DisableExpandButtons();

            CombineMesh();

            WaveManager.Instance.StartWave(_wayPoints);
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
                var neighborWayPoints = _neighborMapArray[i].WayPoints;
                for (var j = 0; j < neighborWayPoints.Length; j++)
                {
                    var dir = (neighborWayPoints[j].position - neighborPos).normalized;

                    if (neighborToNewMapDir != dir) continue;
                    _isConnectedArray[i] = true;
                }
            }
        }

        private void PlaceNewMap()
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

            if (_connectionString == null) return;

            if (_connectionString.Length == 1)
            {
                HandleSingleConnection();
            }
            else
            {
                HandleMultiConnection();
            }
        }

        private void HandleSingleConnection()
        {
            if (_nullMapIndexString == null || CanSpawnPortal())
            {
                _newMapObject = Instantiate(uniqueMap[0], transform);
                _map.Add(_newMapObject);
                return;
            }

            _connectionString += _nullMapIndexString[Random.Range(0, _nullMapIndexString.Length)];
            _connectionString = string.Concat(_connectionString.OrderBy(c => c));
            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);
            _map.Add(_newMapObject);
        }

        private void HandleMultiConnection()
        {
            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);
            _map.Add(_newMapObject);
        }

        private bool CanSpawnPortal()
        {
            var ranProbability = Random.Range(0, 100);
            return ranProbability <= portalSpawnProbability;
        }

        private void SetNewMapForward(Vector3 newMapPos)
        {
            var firstIndex = int.Parse(_connectionString[0].ToString());
            _newMapForward = -_checkDirection[firstIndex];
            _newMapObject.transform.position = newMapPos;
            _newMapObject.transform.forward = _newMapForward;
        }

        private void SetButtonsPosition(Vector3 newMapPos)
        {
            _expandButtonPosHashSet.RemoveWhere(p => p == newMapPos);
            _newMapObject.TryGetComponent(out MapData mapData);
            _newMapWayPoints.Clear();
            for (var i = 0; i < mapData.WayPoints.Length; i++)
            {
                _newMapWayPoints.Add(mapData.WayPoints[i].position);
            }


            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                if (CanAddToList(_newMapWayPoints[i]))
                {
                    _expandButtonPosHashSet.Add(_newMapPosition + _dirToWayPoint * mapSize);
                }
            }
        }

        private void SetWayPoints()
        {
            _newMapObject.TryGetComponent(out MapData mapData);

            for (var i = 0; i < mapData.WayPoints.Length; i++)
            {
                _wayPoints.RemoveWhere(p => p == mapData.WayPoints[i].position);
            }

            for (var i = 0; i < _newMapWayPoints.Count; i++)
            {
                if (CanAddToList(_newMapWayPoints[i]))
                {
                    _wayPoints.Add(_newMapPosition + _dirToWayPoint * mapSize * 0.5f);
                }
            }

            if (mapData.WayPoints.Length == 1)
            {
                _wayPoints.Add(_newMapObject.transform.position);
            }
        }

        //Call When Wave is over
        private void PlaceExpandButtons()
        {
            _expandButtonPosList.Clear();
            _expandButtonPosList = _expandButtonPosHashSet.ToList();

            for (int i = 0; i < _expandButtonPosList.Count; i++)
            {
                _expandButtons[i].transform.position = _expandButtonPosList[i];
                _expandButtons[i].gameObject.SetActive(true);
            }
        }

        private void DisableExpandButtons()
        {
            for (var i = 0; i < _expandButtonPosList.Count; i++)
            {
                if (_expandButtons[i].gameObject.activeSelf) _expandButtons[i].gameObject.SetActive(false);
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
                    meshRenderer.enabled = false;
            }
#if UNITY_EDITOR
            if (saveMeshCount >= 1) return;
            const string path = "Assets/Meshes/Map.asset";
            AssetDatabase.CreateAsset(_meshFilter.mesh, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
#endif
        }

        private bool CanAddToList(Vector3 point)
        {
            _newMapPosition = _newMapObject.transform.position;
            _dirToWayPoint = (point - _newMapPosition).normalized;
            if (_dirToWayPoint == Vector3.zero) return false;
            var ray = new Ray(_newMapPosition, _dirToWayPoint);
            if (Physics.SphereCast(ray, 2, mapSize, groundLayer)) return false;

            var finalPos = _newMapPosition + _dirToWayPoint * mapSize;
            return (finalPos.x >= -maxMapWidth && finalPos.x <= maxMapWidth) ||
                   (finalPos.z >= -maxMapHeight && finalPos.z <= maxMapHeight);
        }
    }
}