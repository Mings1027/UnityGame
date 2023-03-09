using System;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour, IPointerDownHandler
    {
        private Vector3 _checkRangePoint;
        private Collider[] _results;
        private MeshRenderer _meshRenderer;
        private float _range;

        [SerializeField] private InputManager input;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private GameObject towerRangeGameObject;

        private void Awake()
        {
            _results = new Collider[3];
            _meshRenderer = towerRangeGameObject.GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            input.onClosePanelEvent += OffRange;
        }

        private void OnEnable()
        {
            _meshRenderer.enabled = false;
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _meshRenderer.enabled = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _range);
        }

        private void OffRange()
        {
            _meshRenderer.enabled = false;
        }

        public void RangeSetUp(float atkRange)
        {
            _range = atkRange;
            towerRangeGameObject.transform.localScale = new Vector3(_range * 2, 0.1f, _range * 2);
        }

        public (Transform, bool) FindClosestTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, _range, _results, targetLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            if (size <= 0) return (null, false);

            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _results[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _results[i].transform;
            }

            return (nearestEnemy, true);
        }
    }
}