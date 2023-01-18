using System;
using GameControl;
using UnityEngine;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed;

        private Transform target;
        private int wayPointIndex = 0;

        private void Awake()
        {
            target = WayPoints.wayPoints[0];
        }

        private void Update()
        {
            var dir = target.position - transform.position;
            transform.Translate(dir.normalized * (speed * Time.deltaTime),Space.World);
        }
    }
}