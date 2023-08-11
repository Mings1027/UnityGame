using System;
using UnityEngine;

namespace ManagerControl
{
    public class InputManager : MonoBehaviour
    {
        private TowerManager _towerManager;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Vector3 _snappedPos;
        private bool _isPlacingTower;
        private float _inverseGridSize;
        private string _selectedTowerName;
        private bool _canPlace;

        private MeshRenderer _cursorMeshRenderer;

        [SerializeField] private Transform cubeCursor;
        [SerializeField] private float gridSize;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            _inverseGridSize = 1 / gridSize;
            _cursorMeshRenderer = cubeCursor.GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _cursorMeshRenderer.enabled = false;
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
        }

        private void Update()
        {
            if (!_isPlacingTower) return;
            if (Input.GetMouseButton(0))
            {
                SnapToGrid(Input.mousePosition);
                // CheckCanPlace();
                CheckPlacement();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_canPlace)
                {
                    _snappedPos.y = 0.5f;
                    _towerManager.PlaceTower(_selectedTowerName, _snappedPos);
                }

                if (_isPlacingTower)
                {
                    StopPlacement();
                }
            }
        }

        private void StopPlacement()
        {
            _isPlacingTower = false;
            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            enabled = false;
            _snappedPos = Vector3.zero + Vector3.down * 5;
        }

        public void StartPlacement(string towerName)
        {
            _towerManager.ResetUI();
            _isPlacingTower = true;
            _selectedTowerName = towerName;
            _cameraManager.enabled = false;
        }

        private void SnapToGrid(Vector3 touchPos)
        {
            var ray = _cam.ScreenPointToRay(touchPos);
            if (Physics.Raycast(ray, out var hit, _cam.farClipPlane))
            {
                _snappedPos = hit.point;
                _snappedPos.x = Mathf.FloorToInt(_snappedPos.x * _inverseGridSize) * gridSize + gridSize * 0.5f;
                _snappedPos.y = _cursorMeshRenderer.bounds.size.y * 0.5f;
                _snappedPos.z = Mathf.FloorToInt(_snappedPos.z * _inverseGridSize) * gridSize + gridSize * 0.5f;
            }
            else
            {
                _snappedPos = Vector3.zero + Vector3.down * 5;
            }

            cubeCursor.transform.position = _snappedPos + Vector3.up * 0.5f;
        }

        private void CheckPlacement()
        {
            if (Physics.Raycast(_snappedPos + Vector3.up * 10, Vector3.down, out var hit, 100))
            {
                if (hit.collider.CompareTag("NotRoad"))
                {
                    _canPlace = true;
                    _cursorMeshRenderer.enabled = true;
                }
                else
                {
                    _canPlace = false;
                    _cursorMeshRenderer.enabled = false;
                }
            }
        }
    }
}