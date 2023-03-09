using System;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private Camera _cam;
        private Vector3 _arrivalPos;
        private BarracksUnit[] _barracksUnits;
        private Ray _ray;
        private RaycastHit _hit;

        private bool _getUnitDestination;
        private bool _unitMove;

        [SerializeField] private InputManager input;
        [SerializeField] private LayerMask groundLayer;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            isSold = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            isSold = true;
            BarrackUnitSetUp();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _getUnitDestination = true;
        }

        protected override void Update()
        {
            if (!_unitMove) return;
            UnitControl();
        }

        protected override void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, unitMoveRange);
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
            _unitMove = true;
            if (Physics.Raycast(_ray, out _hit, groundLayer))
            {
                if (Vector3.Distance(transform.position, _hit.point) < unitMoveRange)
                {
                    _arrivalPos = _hit.point;
                }
            }
        }

        public override void SetUp(float attackDelay, int unitDamage, int unitHealth)
        {
            base.SetUp(attackDelay, unitDamage, unitHealth);
            UpgradeUnit(unitHealth, unitDamage, attackDelay).Forget();
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

        private async UniTaskVoid UpgradeUnit(int unitHealth, int unitDamage, float attackDelay)
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    await UniTask.Delay(1000, cancellationToken: cts.Token);

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

                    _barracksUnits[i].GetComponent<Health>().InitializeHealth(unitHealth);
                    _barracksUnits[i].UnitSetUp(damage);
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

        protected override void FindTarget()
        {
        }

        protected override void UnitControl()
        {
            var count = 0;
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i].gameObject.activeSelf)
                {
                    count++;
                }
            }

            var angle = 360 / count;

            for (var i = 0; i < count; i++)
            {
                var theta = i * angle * Mathf.Deg2Rad;
                var x = Mathf.Sin(theta);
                var y = Mathf.Cos(theta);
                var pos = new Vector3(x, 0, y);
                _barracksUnits[i]._nav.SetDestination(_arrivalPos + pos);
            }
        }
    }
}