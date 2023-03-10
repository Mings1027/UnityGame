using System;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class BarracksTower : TowerBase
    {
        private Camera _cam;
        private Vector3 _arrivalPos;
        private Ray _ray;
        private RaycastHit _hit;
        private BarracksUnit[] _barracksUnits;

        private bool _getUnitDestination;
        private bool _isMovingUnit;

        [SerializeField] private InputManager input;
        [SerializeField] private float unitMoveRange;

        protected override void Awake()
        {
            base.Awake();
            _barracksUnits = new BarracksUnit[3];
            _cam = Camera.main;
            input.onGetMousePositionEvent += GetUnitDestination;
            input.onClickEvent += MoveUnit;
            input.onClosePanelEvent += () => _getUnitDestination = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            BarrackUnitSetUp();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _getUnitDestination = true;
        }

        private void Update()
        {
            if (!_isMovingUnit) return;
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                _barracksUnits[i].MovingUnit(_arrivalPos);
            }
        }
        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        private void GetUnitDestination(Vector2 moveVec)
        {
            if (!_getUnitDestination) return;
            _ray = _cam.ScreenPointToRay(moveVec);
        }

        private void MoveUnit()
        {
            if (!_getUnitDestination) return;
    
            if (!Physics.Raycast(_ray, out _hit)) return;
            if (!_hit.collider.CompareTag("Ground")) return;
 
            if (Vector3.Distance(transform.position, _hit.point) < unitMoveRange)
            {
                _arrivalPos = _hit.point;
                _isMovingUnit = true;
            }
        }

        public override void Building(float delay, float range, int damage, int health, MeshFilter towerMeshFilter)
        {
            base.Building(delay, range, damage, health, towerMeshFilter);
            UpgradeUnit(delay, range, damage, health).Forget();
        }

        private void BarrackUnitSetUp()
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] == null || !_barracksUnits[i].gameObject.activeSelf) continue;
                _barracksUnits[i].gameObject.SetActive(false);
                _barracksUnits[i].onDeadEvent -= ReSpawn;
                _barracksUnits[i] = null;
            }
        }

        private async UniTaskVoid UpgradeUnit(float delay, float range, int damage, int health)
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    await UniTask.Delay(100, cancellationToken: cts.Token);

                    if (_barracksUnits[i] != null && towerLevel == 4) //이미 스폰됨 && level = 4
                    {
                        _barracksUnits[i].onDeadEvent -= ReSpawn;
                        _barracksUnits[i].gameObject.SetActive(false);
                        var pos = _barracksUnits[i].transform.position;
                        _barracksUnits[i] = null;
                        _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, pos);
                        _barracksUnits[i].onDeadEvent += ReSpawn;
                    }
                    else if (_barracksUnits[i] == null) //스폰 되기 전
                    {
                        _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                        _barracksUnits[i].onDeadEvent += ReSpawn;
                    }

                    _barracksUnits[i].GetComponent<TargetFinder>().SetUp(delay, range, damage);
                    _barracksUnits[i].GetComponent<Health>().InitializeHealth(health);
                }
            }
        }

        private async UniTaskVoid ReSpawnTask()
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                foreach (var t in _barracksUnits)
                {
                    if (t.gameObject.activeSelf) continue;
                    await UniTask.Delay(5000, cancellationToken: cts.Token);
                    t.transform.position = hit.position;
                    t.gameObject.SetActive(true);
                }
            }
        }

        private void ReSpawn()
        {
            if (isSold) return;
            ReSpawnTask().Forget();
        }

        // private void UnitControl()
        // {
        //     var count = 0;
        //     for (var i = 0; i < _barracksUnits.Length; i++)
        //     {
        //         if (_barracksUnits[i].gameObject.activeSelf)
        //         {
        //             count++;
        //         }
        //     }
        //
        //     var angle = 360 / count;
        //
        //     for (var i = 0; i < count; i++)
        //     {
        //         var theta = i * angle * Mathf.Deg2Rad;
        //         var x = Mathf.Sin(theta);
        //         var y = Mathf.Cos(theta);
        //         var pos = new Vector3(x, 0, y);
        //         _barracksUnits[i].MoveUnit(_arrivalPos + pos);
        //     }
        // }
    }
}