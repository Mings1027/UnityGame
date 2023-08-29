using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManagerControl
{
    public class InputManager : MonoBehaviour
    {
        private TowerManager _towerManager;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Vector3 _prevCursorPos;
        private Vector3 _gridPos;
        private bool _isPlacingTower;
        private bool _isUnitTower;
        private string _selectedTowerName;
        private bool _canPlace;

        private Vector3[] _checkDir;

        private MeshRenderer _cursorMeshRenderer;
        private Ray _mouseRay;
        private RaycastHit _mouseRaycastHit;
        [SerializeField] private Image placeTowerButton;
        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private LayerMask towerLayer;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            _cursorMeshRenderer = cubeCursor.GetComponent<MeshRenderer>();
            placeTowerButton.GetComponent<PlaceTowerController>().OnPlaceTower += PlaceTower;

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
            if (!_isPlacingTower) return;

            if (Input.GetMouseButton(0))
            {
                MoveCursor();
                CheckCanPlace();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_isPlacingTower)
                {
                    StopPlacement();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_gridPos + Vector3.up * 5, 1);
        }

        private void StopPlacement()
        {
            _isPlacingTower = false;
            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            placeTowerButton.enabled = false;
            enabled = false;
            _gridPos = Vector3.zero + Vector3.down * 5;
        }

        public void StartPlacement(string towerName, bool isUnitTower)
        {
            if (!_towerManager.EnoughGold())
            {
                StopPlacement();
                return;
            }

            _towerManager.ResetUI();
            _isPlacingTower = true;
            _isUnitTower = isUnitTower;
            _selectedTowerName = towerName;
            _cameraManager.enabled = false;
            placeTowerButton.enabled = true;
        }

        private void MoveCursor()
        {
            _mouseRay = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(_mouseRay, out _mouseRaycastHit, 100);
            var cellPos = grid.WorldToCell(_mouseRaycastHit.point);
            _gridPos = grid.CellToWorld(cellPos) + new Vector3(1.5f, 2f, 1.5f);
            cubeCursor.transform.position = _gridPos;
            placeTowerButton.transform.position = Input.mousePosition;
        }

        private void CheckCanPlace()
        {
            _canPlace = CheckPlacementTile() &&
                        !Physics.CheckSphere(_gridPos, 1, towerLayer) &&
                        (!_isUnitTower || CheckPlaceUnitTower());

            _cursorMeshRenderer.enabled = _canPlace;
        }

        private bool CheckPlacementTile()
        {
            if (Physics.Raycast(_gridPos + Vector3.up * 5, Vector3.down, out var hit, 10))
            {
                if (hit.collider.CompareTag("Placement"))
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
                if (Physics.Raycast(_gridPos + _checkDir[i] * 3 + Vector3.up, Vector3.down,
                        out var hit))
                {
                    if (hit.collider.CompareTag("Ground"))
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
            _gridPos = cubeCursor.transform.position;
            _gridPos.y = 0.5f;
            _towerManager.PlaceTower(_selectedTowerName, _gridPos);
        }
    }
}