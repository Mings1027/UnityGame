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
            AssassinUnit,
            DefenderUnit
        }

        private int _deadUnitCount;
        private string unitTypeName;
        private CancellationTokenSource _cts;
        private Vector3[] _spawnDirections;
        private Vector3 _unitSpawnPosition;

        private FriendlyUnit[] units;

        [SerializeField] private UnitType unitType;
        [SerializeField] private float maxDistance;
        [SerializeField] private float[] unitHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Awake()
        {
            base.Awake();
            units = new FriendlyUnit[3];
            unitTypeName = unitType.ToString();
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
            if (!isSpawn) return;

            isSpawn = false;

            for (var i = 0; i < units.Length; i++)
            {
                units[i].gameObject.SetActive(false);
                units[i] = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            for (var i = 0; i < _spawnDirections.Length; i++)
            {
                Gizmos.DrawSphere(transform.position + _spawnDirections[i] * maxDistance, 0.5f);
            }
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Init()
        {
            base.Init();
            _spawnDirections = new[]
            {
                Vector3.back, Vector3.forward, Vector3.left, Vector3.right
            };
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, attackRangeData, attackDelayData);

            if (!isSpawn)
            {
                isSpawn = true;
                SpawnUnitOnTowerSpawn();
            }

            UnitInit(damageData, attackDelayData);
        }

        private void SpawnUnitOnTowerSpawn()
        {
            // Call Only once when tower spawn
            // ↑ ↓ ← → Four Direction Check Ground and Unit Spawn 
            foreach (var dir in _spawnDirections)
            {
                if (Physics.Raycast(transform.position + dir * maxDistance + Vector3.up, Vector3.down, out var hit))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        _unitSpawnPosition = hit.point;
                        UnitSpawn();
                        break;
                    }
                }

                if (Physics.Raycast(transform.position + dir * maxDistance * 2 + Vector3.up, Vector3.down,
                        out var secondHit))
                {
                    if (secondHit.collider.CompareTag("Ground"))
                    {
                        _unitSpawnPosition = secondHit.point;
                        UnitSpawn();
                        break;
                    }
                }
            }
        }

        private void UnitInit(int damage, float delay)
        {
            var health = unitHealth[TowerLevel];

            for (var i = 0; i < units.Length; i++)
            {
                units[i].Init(damage, delay, health);
            }
        }

        public void UnitMove(Vector3 touchPos)
        {
            for (var i = 0; i < units.Length; i++)
            {
                touchPos = new Vector3(touchPos.x + Random.insideUnitCircle.x, 0,
                    touchPos.z + Random.insideUnitCircle.y);

                units[i].MoveToTouchPos(touchPos);
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

        private void UnitSpawn()
        {
            for (var i = 0; i < units.Length; i++)
            {
                units[i] = null;
                var ranPos = new Vector3(_unitSpawnPosition.x + Random.insideUnitCircle.x, 0,
                    _unitSpawnPosition.z + Random.insideUnitCircle.y);
                units[i] = ObjectPoolManager.Get<FriendlyUnit>(unitTypeName, ranPos);
                units[i].OnDeadEvent += UnitReSpawn;
            }

            ObjectPoolManager.Get(PoolObjectName.UnitSpawnSmoke, _unitSpawnPosition);
        }
    }
}