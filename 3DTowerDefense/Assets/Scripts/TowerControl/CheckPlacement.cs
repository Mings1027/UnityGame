using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace TowerControl
{
    public class CheckPlacement : MonoBehaviour
    {
        private BuildingManager _buildingManager;
        private MeshRenderer _meshRenderer;
        [SerializeField] private bool _isPlaced;

        [SerializeField] private int canPlaceRadius;
        [SerializeField] private Collider[] overlapColliders;
        [SerializeField] private Material[] materials; // 0: can't Place  1: can Place
        [SerializeField] private LayerMask placeCheckLayer;

        private void Awake()
        {
            _buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
            _meshRenderer = GetComponent<MeshRenderer>();

            overlapColliders = new Collider[3];
        }

        private void OnEnable()
        {
            _isPlaced = false;
            _meshRenderer.enabled = true;
            _meshRenderer.material = materials[1];
        }

        private void Update()
        {
            if (_isPlaced) return;
            var count = Physics.OverlapSphereNonAlloc(transform.position, canPlaceRadius, overlapColliders,
                placeCheckLayer);
            if (overlapColliders.Equals(gameObject)) return;
            _meshRenderer.material = count > 1 ? materials[0] : materials[1];
            _buildingManager.canPlace = count <= 1;
        }

        private void OnMouseDown()
        {
            if (UiManager.OnPointer) return;
            SetPlaceTower();
        }

        private void SetPlaceTower()
        {
            if (!_buildingManager.canPlace) return;
            _isPlaced = true;
            _meshRenderer.enabled = false;
        }

        private void OnDrawGizmos()
        {
            if (_isPlaced) return;
            Gizmos.DrawWireSphere(transform.position, canPlaceRadius);
        }
    }
}