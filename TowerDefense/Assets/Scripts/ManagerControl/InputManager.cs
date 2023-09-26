using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class InputManager : Singleton<InputManager>
    {
        private TowerManager _towerManager;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Transform _cursorChild;
        private Vector3 _worldGridPos;
        private TowerType _selectedTowerType;
        private bool _isUnitTower;
        private bool _canPlace;
        private bool _startPlacement;

        private Vector3[] _checkDir;

        private MeshRenderer _cursorMeshRenderer;

        private RaycastHit _unitSpawnRay;
        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private LayerMask towerLayer;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            _cameraManager = CameraManager.Instance;
            _cursorMeshRenderer = cubeCursor.GetComponentInChildren<MeshRenderer>();
            _cursorChild = cubeCursor.GetChild(0);

            _checkDir = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                new Vector3(1, 0, 1), new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1), new Vector3(1, 0, -1)
            };
            _selectedTowerType = TowerType.None;
            _cursorMeshRenderer.enabled = false;
        }

        private void Update()
        {
            if (!_startPlacement) return;

            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase.Equals(TouchPhase.Began) || touch.phase.Equals(TouchPhase.Moved))
            {
                UpdateCursorPosition();
                CheckCanPlace();
            }
            else if (touch.phase.Equals(TouchPhase.Ended))
            {
                _startPlacement = false;
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (!_canPlace) return;
            _startPlacement = false;
            PlaceTower();
            StopPlacement();
            _canPlace = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_cursorChild.position + Vector3.up * 3, 1);
            Gizmos.DrawWireSphere(_cursorChild.position, 0.2f);
            Gizmos.color = Color.green;
            for (int i = 0; i < _checkDir.Length; i++)
            {
                Gizmos.DrawRay(_cursorChild.position + _checkDir[i] * 2 + Vector3.up, Vector3.down * 10);
            }
        }
#endif
        private void StopPlacement()
        {
            _selectedTowerType = TowerType.None;
            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            cubeCursor.position = Vector3.zero + Vector3.down * 2;
            _worldGridPos = Vector3.zero + Vector3.down * 5;
        }

        public void StartPlacement(in TowerType towerType, bool isUnitTower)
        {
            _startPlacement = true;
            _towerManager.OffUI();
            _cameraManager.enabled = false;
            if (!_towerManager.IsEnoughGold(in towerType)) return;
            if (_selectedTowerType == towerType) return;
            _isUnitTower = isUnitTower;
            _selectedTowerType = towerType;
            _cursorMeshRenderer.enabled = false;
        }

        private void UpdateCursorPosition()
        {
            var mousePos = Input.mousePosition;
            mousePos.z = _cam.nearClipPlane;
            var ray = _cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out var hit, 100, placementLayer)) return;
            var cellGridPos = grid.WorldToCell(hit.point);
            _worldGridPos = grid.CellToWorld(cellGridPos);
            cubeCursor.position = _worldGridPos;
        }

        private void CheckCanPlace()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            _canPlace = _startPlacement && Physics.Raycast(ray, out _, 100, placementLayer) &&
                        !Physics.CheckSphere(_cursorChild.position, 0.2f, towerLayer) && CheckPlacementTile() &&
                        (!_isUnitTower || CheckPlaceUnitTower());

            _cursorMeshRenderer.enabled = _canPlace;
        }

        private bool CheckPlacementTile()
        {
            return Physics.Raycast(_cursorChild.position + Vector3.up * 5, Vector3.down, out _unitSpawnRay, 10) &&
                   _unitSpawnRay.collider.CompareTag("Placement");
        }

        private bool CheckPlaceUnitTower()
        {
            for (var i = 0; i < _checkDir.Length; i++)
            {
                if (!Physics.Raycast(_cursorChild.position + _checkDir[i] * 2 + Vector3.up, Vector3.down,
                        out _unitSpawnRay, 10)) continue;
                if (_unitSpawnRay.collider.CompareTag("Ground"))
                {
                    return true;
                }
            }

            return false;
        }

        private void PlaceTower()
        {
            _worldGridPos = _cursorChild.position;
            _worldGridPos.y = 1f;
            _towerManager.InstantiateTower(in _selectedTowerType, in _worldGridPos);
            if (_isUnitTower) _towerManager.SetUnitPosition(_unitSpawnRay.point);
            _towerManager.BuildTower();
        }
    }
}