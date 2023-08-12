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
        private float gridSize;

        [SerializeField] private Transform cubeCursor;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            gridSize = cubeCursor.transform.lossyScale.x;
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
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                SnapToGrid(touch.position);

                CheckPlacement();
            }
            else if (touch.phase == TouchPhase.Ended)
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
            if (!_towerManager.CheckGold())
            {
                StopPlacement();
                return;
            }
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