using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class InputManager : Singleton<InputManager>
    {
        private TowerManager _towerManager;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Transform _cursorChild;
        private Vector3 _worldGridPos;
        private bool _isUnitTower;
        private string _selectedTowerName;
        private bool _canPlace;

        private Vector3[] _checkDir;

        private MeshRenderer _cursorMeshRenderer;

        private RaycastHit _canPlaceHit;
        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private LayerMask towerLayer;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            _cursorMeshRenderer = cubeCursor.GetComponentInChildren<MeshRenderer>();
            _cursorChild = cubeCursor.GetChild(0);
            _towerManager.PlaceTowerController.OnPlaceTower += PlaceTower;

            _checkDir = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                new Vector3(1, 0, 1), new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1), new Vector3(1, 0, -1)
            };
        }

        private void Start()
        {
            _cursorMeshRenderer.enabled = false;
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase is TouchPhase.Began or TouchPhase.Moved)
            {
                UpdateCursorPosition();

                CheckCanPlace();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                StopPlacement();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_cursorChild.position + Vector3.up * 5, 1);
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
            if (_canPlace)
            {
                _selectedTowerName = null;
            }

            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            _towerManager.PlaceTowerController.enabled = false;
            cubeCursor.position = Vector3.zero + Vector3.down * 2;
            enabled = false;
            _worldGridPos = Vector3.zero + Vector3.down * 5;
        }

        public void StartPlacement(string towerName, bool isUnitTower)
        {
            _towerManager.OffUI();
            _cameraManager.enabled = false;
            if (!_towerManager.EnoughGold())
            {
                return;
            }

            if (_selectedTowerName == towerName) return;
            _isUnitTower = isUnitTower;
            _selectedTowerName = towerName;
            _cursorMeshRenderer.enabled = false;
            _towerManager.PlaceTowerController.enabled = true;
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
            _towerManager.PlaceTowerController.transform.position = mousePos;
        }

        private void CheckCanPlace()
        {
            _canPlace = !Physics.CheckSphere(_cursorChild.position, 0.2f, towerLayer) && CheckPlacementTile() &&
                        (!_isUnitTower || CheckPlaceUnitTower());

            _cursorMeshRenderer.enabled = _canPlace;
        }

        private bool CheckPlacementTile()
        {
            if (Physics.Raycast(_cursorChild.position + Vector3.up * 5, Vector3.down, out _canPlaceHit, 10))
            {
                if (_canPlaceHit.collider.CompareTag("Placement"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckPlaceUnitTower()
        {
            for (var i = 0; i < _checkDir.Length; i++)
            {
                if (Physics.Raycast(_cursorChild.position + _checkDir[i] * 2 + Vector3.up, Vector3.down,
                        out _canPlaceHit, 10))
                {
                    if (_canPlaceHit.collider.CompareTag("Ground"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void PlaceTower()
        {
            if (!_canPlace) return;
            _worldGridPos = _cursorChild.position;
            _worldGridPos.y = 1f;
            if (_isUnitTower)
            {
                _towerManager.PlaceUnitTower(_selectedTowerName, _worldGridPos, _canPlaceHit);
            }
            else
            {
                _towerManager.PlaceTower(_selectedTowerName, _worldGridPos);
            }
        }
    }
}