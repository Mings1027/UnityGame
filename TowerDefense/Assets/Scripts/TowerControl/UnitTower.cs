using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private enum UnitType
        {
            Assassin,
            Defender
        }

        private bool _isUnitSpawn;
        private int _deadUnitCount;
        private string _unitTypeName;
        private CancellationTokenSource _cts;
        private Vector3[] _spawnDirections;
        private Vector3 _unitSpawnPosition;

        private FriendlyUnit[] _units;

        public Vector3 unitCenterPos
        {
            get
            {
                var count = 0;
                var center = Vector3.zero;
                for (int i = 0; i < _units.Length; i++)
                {
                    if (_units[i].gameObject.activeSelf)
                    {
                        count++;
                        center += _units[i].transform.position;
                    }
                }

                return center / count;
            }
        }

        public float MoveUnitRange { get; private set; }

        [SerializeField] private UnitType unitType;
        [SerializeField] private float maxDistance;
        [SerializeField] private float[] unitHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Awake()
        {
            base.Awake();
            _units = new FriendlyUnit[3];
            _unitTypeName = unitType.ToString();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _deadUnitCount = 0;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            if (!_isUnitSpawn) return;

            _isUnitSpawn = false;

            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].gameObject.SetActive(false);
                _units[i] = null;
            }
        }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.cyan;
        //     for (var i = 0; i < _spawnDirections.Length; i++)
        //     {
        //         Gizmos.DrawSphere(transform.position + _spawnDirections[i] * maxDistance, 0.5f);
        //     }
        // }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Init()
        {
            base.Init();
            _spawnDirections = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                new Vector3(1, 0, 1), new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1), new Vector3(1, 0, -1)
            };
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            MoveUnitRange = rangeData;

            if (!_isUnitSpawn)
            {
                _isUnitSpawn = true;
                SpawnUnitOnTowerSpawn();
            }

            UnitInit(damageData, attackDelayData);
        }

        private void SpawnUnitOnTowerSpawn()
        {
            // Call Only once when tower spawn
            // ↑ ↓ ← → Four Direction Check Ground and Unit Spawn 
            for (var i = 0; i < _spawnDirections.Length; i++)
            {
                var dir = _spawnDirections[i];
                if (Physics.Raycast(transform.position + dir * maxDistance + Vector3.up, Vector3.down, out var hit))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        _unitSpawnPosition = hit.point;
                        UnitSpawn();
                        break;
                    }
                }
            }
        }

        private void UnitSpawn()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i] = null;
                var ranPos = new Vector3(_unitSpawnPosition.x + Random.insideUnitCircle.x, 0,
                    _unitSpawnPosition.z + Random.insideUnitCircle.y);
                _units[i] = ObjectPoolManager.Get<FriendlyUnit>(_unitTypeName, ranPos);
                _units[i].OnDeadEvent += UnitReSpawn;
                _units[i].towerType = unitType.ToString();
            }

            ObjectPoolManager.Get(PoolObjectName.UnitSpawnSmoke, _unitSpawnPosition);
        }

        private void UnitInit(int damage, float delay)
        {
            var health = unitHealth[TowerLevel];

            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Init(damage, delay, health);
            }
        }

        public void UnitMove(Vector3 touchPos)
        {
            for (var i = 0; i < _units.Length; i++)
            {
                touchPos = new Vector3(touchPos.x + Random.insideUnitCircle.x, 0,
                    touchPos.z + Random.insideUnitCircle.y);

                _units[i].MoveToTouchPos(touchPos);
            }
        }

        private void UnitReSpawn(FriendlyUnit u)
        {
            if (isSold) return;

            _deadUnitCount++;

            if (_deadUnitCount < 3) return;
            _deadUnitCount = 0;
            UnitReSpawnDelay().Forget();
        }

        private async UniTaskVoid UnitReSpawnDelay()
        {
            await UniTask.Delay(5000, cancellationToken: _cts.Token);

            UnitSpawn();
        }
    }
}