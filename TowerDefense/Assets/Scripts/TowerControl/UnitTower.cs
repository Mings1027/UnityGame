using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private int _deadUnitCount;
        private CancellationTokenSource _cts;
        private Vector3[] _spawnDirections;

        private Vector3 _curPos;
        private Vector3 _unitSpawnPosition;

        [SerializeField] private FriendlyUnit[] units;

        [SerializeField] private float maxDistance;
        [SerializeField] private float[] unitHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            for (var i = 0; i < units.Length; i++)
            {
                units[i].transform.position = Vector3.zero;
            }

            isSpawn = false;
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
                        for (var i = 0; i < units.Length; i++)
                        {
                            units[i].OnDeadEvent += UnitReSpawn;
                            UnitSpawnTest(units[i]).Forget();
                        }

                        break;
                    }
                }
            }
        }

        private async UniTaskVoid UnitSpawnTest(Component unit)
        {
            var firstPos = transform.position;
            var secondPos = (_unitSpawnPosition + firstPos) * 1f + new Vector3(0, 5, 0);
            float lerp = 0;
            while (lerp < 1)
            {
                lerp += Time.deltaTime;
                firstPos = Vector3.Lerp(firstPos, secondPos, lerp);
                secondPos = Vector3.Lerp(secondPos, _unitSpawnPosition, lerp);
                _curPos = Vector3.Lerp(firstPos, secondPos, lerp);
                unit.transform.position = _curPos;
                await UniTask.Yield();
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
                units[i].MoveToTouchPos(touchPos);
            }
        }

        private void UnitReSpawn(FriendlyUnit u)
        {
            if (isSold) return;
            if (u.GetComponent<UnitHealth>().IsDead)
            {
                _deadUnitCount++;
            }

            if (_deadUnitCount < 3) return;
            _deadUnitCount = 0;
            UnitReSpawnDelay().Forget();
        }

        private async UniTaskVoid UnitReSpawnDelay()
        {
            await UniTask.Delay(5000, cancellationToken: _cts.Token);

            for (var i = 0; i < units.Length; i++)
            {
                units[i].transform.position = transform.position;
                units[i].gameObject.SetActive(true);
            }

            for (var i = 0; i < units.Length; i++)
            {
                units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }
    }
}