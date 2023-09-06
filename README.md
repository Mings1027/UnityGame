Random Map Tower Defense

![스크린샷 2023-09-05 오후 4 11 17](https://github.com/Mings1027/UnityGame/assets/100500113/22cd8fc8-9953-4bd9-8be4-adb24eb22ed7)

```
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
        [SerializeField] private int mapCount;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var way in _wayPoints)
            {
                Gizmos.DrawSphere(way, 1);
            }
        }

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

                _expandButtons[index].ExpandMap();
                await UniTask.Yield();
                PlaceExpandButtons();
            }
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

            // WaveManager.Instance.StartWave(_wayPoints.ToArray());
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
```
# 1. Awake
## 1. ComponentInit();
MeshCombine을 위한 MeshFilter, MeshRenderer 컴포넌트 변수 할당
## 2. MapDataInit();
맵 확장에 필요한 기본 데이터 초기화

# 2. Start 함수
## 1. PlaceStartMap();
게임 시작 시 원점위치에 첫번째 맵 생성
## 2. WaveManager.Instance.OnPlaceExpandButton += PlaceExpandButtons;
WaveManager에서 Enemy를 스폰하는데 스폰된 마지막 Enemy가 사라질때 다음웨이브에 맵 확장을 위해
PlaceExpandButtons 함수를 실행함.

# 3. 게임시작 버튼
## 게임 시작 버튼을 누르면 GenerateInitMap() 호출됨.
### 1. InitExpandButtonPosition()
_connectionString은 _directionMappingDic에서 랜덤선택해서 키를 저장하고 그 벨류의 맵을 생성함

# 4. ExpandMap(Vector3 newMapPos) 핵심 로직
앞으로 나올 파라미터 newMapPos 는 플레이어가 누른 확장버튼의 월드 포지션이다.

## 1. InitAdjacentState
확장시킬 곳 즉, 다음 맵이 생기는곳을 중심으로 연결정보를 저장하는 변수 초기화

## 2. CheckNeighborMap(Vector3 newMapPos)
확장된 맵의 중심에서 네방향에 맵이 존재하는지 체크한다.
존재하면 _isNullMapArray[i] = true 를 해준다.

## 3. CheckConnectedDirection(Vector3 newMapPos)
맵이 존재하는 곳과 확장된 맵이 이어져 있는지 체크한다.
이어져있다면 _isConnectedArray[i] = true 해준다.

## 4. CheckConnection()
위에서 체크한 두 배열 _isNullMapArray, _isConnectedArray을 이용해 
_isConnectedArray[i] = true 인 곳의 i 번째 인덱스를 _connectionString에 저장한다.
_isNullMapArray[i] = true 인 곳은 현재 맵이 없는 방향이기 때문에 길을 넣을지 말지 랜덤선택한다.

## 5. PlaceNewMap(Vector3 newMapPos)
_connectionString을 키로 _mapDictionary 벨류를 가져와 Instantiate해준다.

### 1. IfSingleConnection()
_connectionString.Length == 1 일 때 실행된다.


## 6. SetNewMapForward()
var firstIndex = int.Parse(_connectionString[0].ToString());
_newMapForward = -_checkDirection[firstIndex];

_connectionString 첫번째 인덱스로 확장시킬 맵의 forward를 정한다.

이유는 4. CheckConnection()를 보면 _connectionString에 저장하는것이 0,1,2,3 중에 선택되는데 
이는 순서대로 Vector3.back, Vector3.forward, Vector3.left, Vector3.right이다. 그리고 _connectionString의 첫번째 원소는 처음으로 연결된 방향을 나타낸다. 그 방향이 확장된 맵의 back 이므로 -1을 곱해주면 그 방향이 forward가 된다.

## 7. 