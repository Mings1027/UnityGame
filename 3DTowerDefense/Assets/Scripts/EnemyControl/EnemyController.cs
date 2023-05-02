using System;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public class EnemyController : MonoBehaviour
    {
        private Animator _anim;
        private NavMeshAgent _nav;

        private Vector3 _prevWayPoint;
        private Vector3 _curWayPoint;
        private int _wayPointIndex;
        private bool _canMove;

        public bool StartMove { get; set; }

        [SerializeField] private float moveSpeed;

        private static readonly int Jump = Animator.StringToHash("Jump");

        public event Action<EnemyController> moveToNextWayPointEvent;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _nav = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _nav.speed = moveSpeed;
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            moveToNextWayPointEvent = null;
        }

        private void Update()
        {
            if (!StartMove) return;
            Move();

            // if (_canMove)
            //     PlayAnimation();
            //
            // CheckNextPoint();
        }

        private void Move()
        {
            _nav.SetDestination(_curWayPoint);
            print("왜안가");
        }

        private void PlayAnimation()
        {
            _canMove = false;
            _anim.SetTrigger(Jump);
        }

        private void CheckNextPoint()
        {
            if (_nav.remainingDistance <= _nav.stoppingDistance)
            {
                _canMove = true;
                moveToNextWayPointEvent?.Invoke(this);
            }
        }

        public void Init( Vector3 secondWay)
        {
            // _prevWayPoint = firstWay;
            _curWayPoint = secondWay;
        }

        public void SetDestination(Transform[] waypoint)
        {
            _prevWayPoint = waypoint[_wayPointIndex].position;
            if (waypoint[_wayPointIndex++] == waypoint[^1])
            {
                gameObject.SetActive(false);
                return;
            }

            _curWayPoint = waypoint[_wayPointIndex].position;
        }
    }
}