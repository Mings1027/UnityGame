Random Map Tower Defense

![스크린샷 2023-09-05 오후 4 11 17](https://github.com/Mings1027/UnityGame/assets/100500113/22cd8fc8-9953-4bd9-8be4-adb24eb22ed7)

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