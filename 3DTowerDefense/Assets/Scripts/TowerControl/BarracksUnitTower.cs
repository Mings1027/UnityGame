using GameControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksUnitTower : UnitTower
    {
        private Camera _cam;
        private Vector3 _pos;
        private Collider[] _targetColliders;

        [SerializeField] private LayerMask moveAreaLayer;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
            units = new FriendlyUnit[3];
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
                units[i].Target = _targetColliders[i].transform;
                units[i].IsTargeting = true;
            }
        }

        public bool Move()
        {
            var ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);
            if (!Physics.Raycast(ray, out var hit, moveAreaLayer)) return false;
            foreach (var t in units)
            {
                t.GoToTargetPosition(hit.point);
            }

            return true;
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float delay)
        {
            for (var i = 0; i < units.Length; i++)
            {
                if (units[i] != null) units[i].gameObject.SetActive(false);

                UnitSpawn(i, UnitHealth);
                units[i].Init(minDamage, maxDamage, delay);
            }
        }

        protected override void UnitSpawn(int i, int health)
        {
            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";
            var ranPos = hit.position + Random.insideUnitSphere * 5f;
            units[i] = StackObjectPool.Get<BarracksUnit>(unitName, ranPos);
            units[i].GetComponent<Health>().Init(health);
            units[i].onDeadEvent += ReSpawn;
        }
    }
}