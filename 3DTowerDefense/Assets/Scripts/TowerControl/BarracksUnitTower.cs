using DataControl;
using GameControl;
using UnitControl.EnemyControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksUnitTower : UnitTower
    {
        private Camera _cam;

        [SerializeField] private LayerMask moveAreaLayer;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
            units = new FriendlyUnit[3];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }
        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        public bool Move()
        {
            var ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);
            if (!Physics.Raycast(ray, out var hit, moveAreaLayer)) return false;
            unitsPosition = hit.point;
            foreach (var t in units)
            {
                t.GoToTargetPosition(hit.point);
            }

            return true;
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float delay, int health)
        {
            for (var i = 0; i < units.Length; i++)
            {
                if (units[i] != null) units[i].gameObject.SetActive(false);

                UnitSpawn(i);
                units[i].Init(minDamage, maxDamage, delay, health);
            }
        }

        protected override void UnitSpawn(int i)
        {
            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            unitsPosition = hit.position;
            var unitName = IsUniqueTower ? PoolObjectName.SpearManUnit : PoolObjectName.SwordManUnit;
            var ranPos = hit.position + Random.insideUnitSphere * 5f;
            units[i] = StackObjectPool.Get<BarracksUnit>(unitName, ranPos);
            units[i].onDeadEvent += ReSpawn;
        }
    }
}