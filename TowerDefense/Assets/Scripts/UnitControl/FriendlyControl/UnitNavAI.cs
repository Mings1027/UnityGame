using System;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl.FriendlyControl
{
    public class UnitNavAI : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private bool stopMove;
        public Func<bool> isStopped { get; private set; }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            isStopped = () =>
                Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance;
        }

        private void Update()
        {
            isStopped = () =>
                Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance;
        }
    }
}