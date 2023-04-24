using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.AI;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace UnitControl.EnemyControl
{
    public class GridEnemy : MonoBehaviour
    {
        private Camera cam;
        private NavMeshAgent nav;
        private Vector3 targetPos;

        [SerializeField] private InputManager input;
        [SerializeField] private float moveSpeed = 2f;

        private void Start()
        {
            cam = Camera.main;
            // input.onMoveUnitEvent += Move;
        }

        private void Move()
        {
            
        }
    }
}