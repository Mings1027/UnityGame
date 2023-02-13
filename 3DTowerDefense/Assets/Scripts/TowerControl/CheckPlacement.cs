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
        [SerializeField] private Material[] materials; // 0: can't Place  1: can Place  2: default Material
        [SerializeField] private LayerMask groundLayer;

        private void Awake()
        {
            _buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
            _meshRenderer = GetComponent<MeshRenderer>();
            materials[2] = _meshRenderer.material;

            overlapColliders = new Collider[3];
        }

        private void OnEnable()
        {
            _isPlaced = false;
            _meshRenderer.enabled = true;
            _meshRenderer.material = materials[1];
        }

        private void FixedUpdate()
        {
            if (_isPlaced) return;
            var count = Physics.OverlapSphereNonAlloc(transform.position, canPlaceRadius, overlapColliders,
                ~groundLayer);

            _meshRenderer.material = count > 1 ? materials[0] : materials[1];
            _buildingManager.canPlace = count <= 1;
        }

        private void OnMouseDown()
        {
            if (UiManager.OnPointer || !_buildingManager.canPlace) return;
            SetPlaceTower();
        }

        private void SetPlaceTower()
        {
            _isPlaced = true;
            _meshRenderer.material = materials[2];
        }

        private void OnDrawGizmos()
        {
            if (_isPlaced) return;
            Gizmos.DrawWireSphere(transform.position, canPlaceRadius);
        }
    }
}