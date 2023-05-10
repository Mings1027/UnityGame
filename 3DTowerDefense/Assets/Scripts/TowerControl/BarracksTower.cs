using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksTower : TowerUnitAttacker
    {
        private Camera _cam;
        private int _deadUnitCount;
        private Vector3 _pos;
        private BarracksUnit[] _barracksUnits;

        public int UnitHealth { get; set; }

        [SerializeField] private LayerMask groundLayer;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
            _barracksUnits = new BarracksUnit[3];
        }

        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        public void MoveUnit()
        {
            var ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);
            if (!Physics.Raycast(ray, out var hit, groundLayer)) return;
            if (Vector3.Distance(transform.position, hit.point) < TowerRange)
            {
                foreach (var t in _barracksUnits)
                {
                    t.GoToTargetPosition(hit.point);
                }
            }
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

        protected override void UnitUpgrade(int minDamage, int maxDamage, float range, float delay)
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] != null) _barracksUnits[i].gameObject.SetActive(false);

                UnitSpawnAndInit(i, UnitHealth);
                _barracksUnits[i].UnitInit(minDamage, maxDamage, delay);
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