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
        private Vector3 _gridPos;
        private bool _isPlacingTower;
        private string _selectedTowerName;
        private bool _canPlace;

        private MeshRenderer _cursorMeshRenderer;

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
            var eventTrigger = placeTowerButton.GetComponent<EventTrigger>();
            var entryDrop = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drop
            };
            entryDrop.callback.AddListener((e) => PlaceTower());
            eventTrigger.triggers.Add(entryDrop);
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
                GetPlacementPos();
                MoveTowerButton();
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_gridPos, 1f);
        }

        private void StopPlacement()
        {
            _isPlacingTower = false;
            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            enabled = false;
            _gridPos = Vector3.zero + Vector3.down * 5;
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

        private void GetPlacementPos()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, 100, placementLayer))
            {
                _canPlace = false;
                _cursorMeshRenderer.enabled = false;
                return;
            }

            var cellPos = grid.WorldToCell(hit.point);
            _gridPos = grid.CellToWorld(cellPos) + new Vector3(1.5f, 2f, 1.5f);

            _canPlace = !Physics.CheckSphere(_gridPos, 1, towerLayer);
            _cursorMeshRenderer.enabled = _canPlace;

            cubeCursor.transform.position = _gridPos;
        }

        private void MoveTowerButton()
        {
            placeTowerButton.transform.position = Input.mousePosition;
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