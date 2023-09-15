Random Map Tower Defense

![스크린샷 2023-09-05 오후 4 11 17](https://github.com/Mings1027/UnityGame/assets/100500113/22cd8fc8-9953-4bd9-8be4-adb24eb22ed7)

# 프로젝트 설명

## 적들이 중심에 있는 성으로 몰려오는것을 막는 랜덤맵 타워디펜스 입니다.

# 1. Awake
## 1. ComponentInit();
MeshCombine을 위한 MeshFilter, MeshRenderer 컴포넌트 변수 할당

## 2. MapDataInit();
맵 확장에 필요한 기본 데이터 초기화

<details>
<summary>코드보기</summary>

```c#
        private void Awake()
        {
            ComponentInit();
            MapDataInit();
        }

        private void ComponentInit()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _obstacleMeshFilter = obstacleMesh.GetComponent<MeshFilter>();
            _obstacleMeshRenderer = obstacleMesh.GetComponent<MeshRenderer>();
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
            _obstacleMeshFilters = new List<MeshFilter>(150);

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
                { "01", "S" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "S" },
                { "012", "SL" }, { "013", "SR" }, { "023", "LR" }, { "123", "LR" },
                { "0123", "SLR" }
            };

            _wayPointsHashSet = new HashSet<Vector3>();
        }

```
</details>


# 2. Start
## 1. PlaceStartMap();
게임 시작 시 원점위치에 첫번째 맵 생성
_directionMappingDic의 키들을 배열로 받아서 랜덤 인덱스를 하나 뽑아 _connectionString에 저장함
_directionMappingDic[_connectionString]를 키로 하는 _mapDictionary에서 벨류를 찾아 맵 하나를 생성함.
SetNewMapForward로 방금 생성시킨 맵의 forward를 정해줌.
PlaceObstacle로 랜덤위치에 obstacle생성함.
_map리스트에 생성된 맵 넣어줌.

## 2. WaveManager.Instance.OnPlaceExpandButton += PlaceExpandButtons;
WaveManager에서 Enemy를 스폰하는데 스폰된 마지막 Enemy가 사라질때 다음웨이브에 맵 확장을 위해
PlaceExpandButtons 함수를 실행함.

## 3. GenerateInitMap()
TowerManager.Instance.transform.GetComponentInChildren<MainMenuUIController>().OnGenerateInitMapEvent += GenerateInitMap;
MainMenuUIController는 게임 제일 처음 Start버튼과 카메라를 회전시키기 위해 존재하며 Start버튼을 누른뒤 필요없기 때문에 Destroy해준다.
Start버튼을 누를 때 GenerateInitMap 호출을 위해 이벤트에 등록 시켜주었다.
<details>
<summary>GenerateInitMap함수 보기</summary>

### 1. InitExpandButtonPosition()
위에서 저장한 _connectionString을 foreach를 돌며 각 char를 string변환하고 다시 int로 변환해 index로 활용함.
_expandButtonPosHashSet에 _checkDirection[index]를 저장함.

### 2. PlaceExpandButtons()
웨이브가 끝날 때마다 호출되며 위에서 저장한 _expandButtonPosHashSet를 foreach를 돌며 각 원소를 위치로 하는 expandButton을 배치함
이 버튼은 UI가 아닌 3D오브젝트이며 눌렀을때 맵 확장을 해주기 때문에 ExpandMap함수를 버튼의 스크립트에 있는 이벤트에 등록해줌

<details>
<summary>코드보기</summary>

```c#

        private void Start()
        {
            PlaceStartMap();

            WaveManager.Instance.OnPlaceExpandButton += PlaceExpandButtons;

            TowerManager.Instance.transform.GetComponentInChildren<MainMenuUIController>().OnGenerateInitMapEvent +=
                GenerateInitMap;
        }

        private void PlaceStartMap()
        {
            var ranIndex = Random.Range(0, _directionMappingDic.Count);
            _connectionString = _directionMappingDic.Keys.ToArray()[ranIndex];

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);

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
```
</details>
</details>

# 3. ExpandMap(Vector3 newMapPos) 핵심 로직
앞으로 나올 파라미터 newMapPos 플레이어가 누른 확장버튼의 월드 포지션이며 눌렀을 때 생성된 맵의 위치와 같기 때문에 newMap이라 부르겠다

## 1. InitAdjacentState
newMap 중심으로 연결정보를 저장하는 변수 초기화

## 2. CheckNeighborMap(Vector3 newMapPos)
newMap 중심에서 네방향에 맵이 존재하는지 체크한다.
앞으로 존재하는 맵을 neighborMap이라 부르겠다.

존재하지 않으면 _isNullMapArray[i] = true 해주고 true인 인덱스 i를 _nullMapIndexString에 넣어준다.

## 3. CheckConnectedDirection(Vector3 newMapPos)
neighborMap에서 newMap으로의 방향과 neighborMap에서 neighborMap의 wayPoint로의 방향이 같다면
이는 neighborMap과 newMap이 이어져있다는것을 의미한다.

이어져있다면 _isConnectedArray[i] = true 해준다.

## 4. CheckConnection()
위에서 체크한 두 배열 _isNullMapArray, _isConnectedArray을 이용해 
_isConnectedArray[i] = true 인 인덱스 i 를 _connectionString에 넣어준다.
_isNullMapArray[i] = true 인 곳은 현재 맵이 없는 방향이기 때문에 길을 넣을지 말지 랜덤선택한다.

## 5. PlaceNewMap(Vector3 newMapPos)
_connectionString을 키로 _mapDictionary 벨류를 가져와 Instantiate해준다.

### 1. IfSingleConnection()
_connectionString.Length == 1 일 때
아까 확인한 _isNullMapArray가 전부 false거나 CanSpawnPortal == true 이면 포탈이 생성하고 return한다.
아니라면 _nullMapIndexString원소 중 하나를 _connectionString넣고 _connectionString을 OrderBy 정렬한 후 
_directionMappingDic[_connectionString]를 키로 _mapDictionary에서 가져와 맵을 Instantiate한다.

### 2. IfMultipleConnection()
_connectionString.Length != 1 일 때
_directionMappingDic[_connectionString]를 키로 _mapDictionary에서 가져와 맵을 Instantiate한다.


## 6. SetNewMapForward()

_connectionString 첫번째 인덱스로 newMap의 forward를 정한다.

이 작업을 하는 이유는 4. CheckConnection()에서 _connectionString에 저장하는것이 0,1,2,3 중에 선택되는데 
이는 순서대로 Vector3.back, Vector3.forward, Vector3.left, Vector3.right이다. 그리고 _connectionString의 첫번째 원소는 처음으로 연결된 방향을 나타낸다. 그 방향이 확장된 맵의 back 이므로 -1을 곱해주고 forward로 만든다.

## 7. PlaceObstacle(MapData map)

생성한 맵에 placementTile중 랜덤 개수만큼 for를 돌며 다시 placementTile랜덤 인덱스를 뽑아 RandomPlaceObstacle 호출
방금 뽑은 인덱스는 placementTile리스트에서 삭제함

### 1. RandomPlaceObstacle(MapData map, Vector3 center)

위에서 뽑은 placementTile 위치에서 대각 사방면으로 obstacle을 설치할지 랜덤선택함

## 8. RemovePoints(Vector3 newMapPos)

_newMapWayPoints.Clear()하고 생성한 맵의 웨이포인트들을 _newMapWayPoints에 저장함
_wayPointsHashSet에서 _newMapWayPoints각각이 존재하면 지우고
_expandButtonPosHashSet에서 newMapPos이 존재하면 지움

## 9. SetPoints()

_newMapWayPoints.Count == 1 면 길이 없는 Portal Map 이므로 포탈위치를 _wayPointsHashSet.Add() 하고 return 함

아니라면 CanAddWayPoints가 true면 _wayPointsHashSet에 저장하고 CheckLimitMap가 true면 _expandButtonPosHashSet까지 저장함

### 1. CanAddWayPoints(Vector3 newWayPoint)

newMap에서 newMapWayPoint 방향에 맵이 없을때 return true 

### 2. CheckLimitMap(Vector3 newWayPoint)

newMapWayPoint의 x or z 위치가 setMapSize보다 작을때 return true

## 10. DisableExpandButtons()

ExpandMap이 호출되었다는건 웨이브가 시작되었다는 것이기 때문에 ExpandButton들을 안보이게 해주어야함

## 11. CombineMesh()

최적화를 위해 맵들의 메시를 하나로 묶어줌

## 12. CombineObstacleMesh()

맵 위에 생성되었던 obstacle들의 메시도 하나로 묶어줌


<details>
<summary>전체코드보기</summary>

```c#
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using ManagerControl;
using UIControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class MapController : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshFilter _obstacleMeshFilter;
        private MeshRenderer _obstacleMeshRenderer;

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
        private List<MeshFilter> _obstacleMeshFilters;

        private HashSet<Vector3> _expandButtonPosHashSet;
        private List<Vector3> _expandButtonPosList;

        private List<ExpandMapButton> _expandButtons;

        private MapData[] _neighborMapArray;

        private bool[] _isNullMapArray;
        private bool[] _isConnectedArray;

        private Dictionary<string, GameObject> _mapDictionary;
        private Dictionary<string, string> _directionMappingDic;

        private HashSet<Vector3> _wayPointsHashSet;

        [SerializeField] private int mapSize;
        [SerializeField] private GameObject[] mapPrefabs;
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject[] uniqueMap;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField, Range(0, 100)] private int portalSpawnProbability;
        [SerializeField] private int maxSize;
        [SerializeField] private Transform obstacleMesh;
#if UNITY_EDITOR
        [SerializeField] private int mapCount;
#endif

        #region Unity Event

        private void Awake()
        {
            ComponentInit();
            MapDataInit();
        }

        private void Start()
        {
            PlaceStartMap();

            WaveManager.Instance.OnPlaceExpandButton += PlaceExpandButtons;

            TowerManager.Instance.transform.GetComponentInChildren<MainMenuUIController>().OnGenerateInitMapEvent +=
                GenerateInitMap;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
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
            _checkDirection = new[]
            {
                Vector3.back * mapSize, Vector3.forward * mapSize, Vector3.left * mapSize,
                Vector3.right * mapSize
            };
            _map = new List<GameObject>();
            _newMapWayPoints = new List<Vector3>(4);
            _meshFilters = new List<MeshFilter>(150);
            _obstacleMeshFilters = new List<MeshFilter>(150);

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
                { "01", "S" }, { "02", "L" }, { "03", "R" }, { "12", "R" }, { "13", "L" }, { "23", "S" },
                { "012", "SL" }, { "013", "SR" }, { "023", "LR" }, { "123", "LR" },
                { "0123", "SLR" }
            };

            _wayPointsHashSet = new HashSet<Vector3>();
        }

        private void PlaceStartMap()
        {
            var ranIndex = Random.Range(0, _directionMappingDic.Count);
            _connectionString = _directionMappingDic.Keys.ToArray()[ranIndex];

            _newMapObject = Instantiate(_mapDictionary[_directionMappingDic[_connectionString]], transform);

            _newMapObject.TryGetComponent(out MapData mapData);
            SetNewMapForward(mapData);
            PlaceObstacle(mapData);
            _map.Add(_newMapObject);
        }

        private void GenerateInitMap()
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
            //     _expandButtons[index].Expand();
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

        #endregion

        private void ExpandMap(Vector3 newMapPos)
        {
            InitAdjacentState();

            CheckNeighborMap(newMapPos);

            CheckConnectedDirection(newMapPos);

            CheckConnection();

            PlaceNewMap(newMapPos);

            ObjectPoolManager.Get(StringManager.ExpandMapSmoke, newMapPos);
            _newMapObject.TryGetComponent(out MapData mapData);

            SetNewMapForward(mapData);

            PlaceObstacle(mapData);

            RemovePoints(mapData, newMapPos);

            SetPoints();

            DisableExpandButtons();

            CombineMesh();

            CombineObstacleMesh();
            WaveManager.Instance.StartWave(_wayPointsHashSet.ToArray());
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

        private void SetNewMapForward(MapData map)
        {
            var firstIndex = int.Parse(_connectionString[0].ToString());
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
                if (CanAddWayPoints(_newMapWayPoints[i]))
                {
                    _wayPointsHashSet.Add(_newMapPosition + _dirToWayPoint * mapSize * 0.5f);
                    if (CheckLimitMap(_newMapWayPoints[i]))
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
            SoundManager.Instance.PlayBGM(StringManager.WaveBreak);
            _expandButtonPosList.Clear();
            _expandButtonPosList = _expandButtonPosHashSet.ToList();

            for (var i = 0; i < _expandButtonPosList.Count; i++)
            {
                _expandButtons.Add(ObjectPoolManager.Get<ExpandMapButton>(StringManager.ExpandButton,
                    _expandButtonPosList[i], Quaternion.Euler(0, 45, 0)));
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
            var setMapSize = maxSize * mapSize;
            return newWayPoint.x >= -setMapSize && newWayPoint.x <= setMapSize &&
                   newWayPoint.z >= -setMapSize && newWayPoint.z <= setMapSize;
        }
    }
}
```
</details>

























