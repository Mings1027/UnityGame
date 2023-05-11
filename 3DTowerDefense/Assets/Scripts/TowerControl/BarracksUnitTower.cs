using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksUnitTower : UnitTower
    {
        private Camera _cam;
        private int _deadUnitCount;
        private Vector3 _pos;
        private BarracksUnit[] _barracksUnits;
        private Collider[] _targetColliders;

        public int UnitHealth { get; set; }

        [SerializeField] private LayerMask moveAreaLayer;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
            _barracksUnits = new BarracksUnit[3];
            _targetColliders = new Collider[3];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(FindingTarget), 1, 1);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }
        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        protected override void FindingTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, _targetColliders, TargetLayer);
            if (size <= 0) return;

            for (var i = 0; i < size; i++)
            {
                if (_barracksUnits[i].IsMatching) continue;
                _barracksUnits[i].IsMatching = true;
                _barracksUnits[i].Target = _targetColliders[i].transform;
                _barracksUnits[i].IsTargeting = true;
            }
        }

        public bool Move()
        {
            var ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);
            if (!Physics.Raycast(ray, out var hit, moveAreaLayer)) return false;
            foreach (var t in _barracksUnits)
            {
                t.GoToTargetPosition(hit.point);
            }

            return true;
        }

        protected override void UnitDisable()
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] != null && _barracksUnits[i].gameObject.activeSelf)
                {
                    _barracksUnits[i].gameObject.SetActive(false);
                    _barracksUnits[i] = null;
                }
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float delay)
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] != null) _barracksUnits[i].gameObject.SetActive(false);

                UnitSpawnAndInit(i, UnitHealth);
                _barracksUnits[i].Init(minDamage, maxDamage, delay);
            }
        }

        private void ReSpawn(BarracksUnit b)
        {
            if (isSold) return;

            if (b.GetComponent<Health>().IsDead)
            {
                _deadUnitCount++;
            }

            if (_deadUnitCount < 3) return;

            ReSpawnTask().Forget();
        }

        private async UniTaskVoid ReSpawnTask()
        {
            _deadUnitCount = 0;
            await UniTask.Delay(5000);
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                UnitSpawnAndInit(i, UnitHealth);
            }
        }

        private void UnitSpawnAndInit(int i, int health)
        {
            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";
            var ranPos = hit.position + Random.insideUnitSphere * 5f;
            _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, ranPos);
            _barracksUnits[i].GetComponent<Health>().Init(health);
            _barracksUnits[i].onDeadEvent += ReSpawn;
        }
    }
}