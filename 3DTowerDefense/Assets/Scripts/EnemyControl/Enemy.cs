using System;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _target;

        [SerializeField] private float range;
        [SerializeField] private LayerMask attackAbleLayer;
        [SerializeField] private Collider[] targets;
        public Transform destination;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            targets = new Collider[5];
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, range, targets, attackAbleLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToUnit = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distanceToUnit < shortestDistance)
                {
                    shortestDistance = distanceToUnit;
                    nearestTarget = targets[i].transform;
                }
            }

            _target = nearestTarget != null && shortestDistance <= range ? nearestTarget : null;
        }

        private void Update()
        {
            _agent.SetDestination(destination.position);
            if (Vector3.Distance(transform.position, destination.position) <= 0.2f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}