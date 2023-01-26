using GameControl;
using Unity.Mathematics;
using UnityEngine;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private int speed;
        [Range(0, 1)] [SerializeField] private float turnSpeed;

        private Transform _target;
        private int _wayPointIndex;

        private void Update()
        {
            MoveToWayPoint();
            if (Vector3.Distance(transform.position, _target.position) <= 0.4f)
            {
                MoveToNextWayPoint();
            }
        }

        private void OnDisable()
        {
            _wayPointIndex = 0;
            _target = WaveSpawner.WayPoints[_wayPointIndex];
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void MoveToWayPoint()
        {
            var dir = (_target.position - transform.position).normalized;
            if (dir == Vector3.zero) return;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }

        private void MoveToNextWayPoint()
        {
            if (_wayPointIndex >= WaveSpawner.WayPoints.Length - 1)
            {
                gameObject.SetActive(false);
                return;
            }

            _wayPointIndex++;
            _target = WaveSpawner.WayPoints[_wayPointIndex];
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, _target.position - transform.position);
        }
    }
}