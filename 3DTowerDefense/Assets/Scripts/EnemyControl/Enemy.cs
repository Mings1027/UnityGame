using System;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        public event Action<Enemy> OnMoveNexPoint;
        private NavMeshAgent _agent;
        public int WayPointIndex { get; set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_agent.remainingDistance <= 0.2f)
            {
                OnMoveNexPoint?.Invoke(this);
            }
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