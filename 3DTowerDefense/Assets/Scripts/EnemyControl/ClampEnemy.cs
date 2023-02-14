using System;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public class ClampEnemy : MonoBehaviour
    {
        public event Action<ClampEnemy> OnMoveNexPoint;

        private NavMeshAgent _agent;
        public int WayPointIndex { get; set; }

        [SerializeField] private Transform crystal;
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.SetDestination(crystal.position);
        }

        private void Update()
        {
            // if (_agent.remainingDistance > _agent.stoppingDistance)
            // {
            //     _agent.SetDestination(crystal.position);
            // }
            // if (_agent.remainingDistance <= 0.2f)
            // {
            //     OnMoveNexPoint?.Invoke(this);
            // }
        }

        private void FixedUpdate()
        {
            _agent.SetDestination(crystal.position);
        }

        public void SetMovePoint(Vector3 pos)
        {
            _agent.SetDestination(pos);
        }

        private void OnDisable()
        {
            WayPointIndex = 0;
            StackObjectPool.ReturnToPool(gameObject);
            OnMoveNexPoint = null;
        }
    }
}