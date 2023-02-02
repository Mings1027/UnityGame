using System;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public class CheckPlacement : MonoBehaviour
    {
        private BuildingController _buildingController;
        private MeshRenderer _meshRenderer;
        private Material _defaultMaterial;
        private bool _isPlaced;

        [SerializeField] private Material[] materials; // 0: can't Place  1: can Place

        private void Awake()
        {
            _buildingController = GameObject.Find("BuildingManager").GetComponent<BuildingController>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _defaultMaterial = _meshRenderer.material;
        }

        private void OnEnable()
        {
            _meshRenderer.material = materials[1];
            _isPlaced = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPlaced) return;
            if (!other.CompareTag("Tower")) return;

            _buildingController.canPlace = false;
            _meshRenderer.material = materials[0];
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isPlaced) return;
            if (!other.CompareTag("Tower")) return;

            _buildingController.canPlace = true;
            _meshRenderer.material = materials[1];
        }

        public void SetPlaceTower()
        {
            _isPlaced = true;
            _meshRenderer.material = _defaultMaterial;
        }
    }
}