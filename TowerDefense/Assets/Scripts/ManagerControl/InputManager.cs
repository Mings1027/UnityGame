using CustomEnumControl;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ManagerControl
{
    public class InputManager : MonoBehaviour
    {
        public TowerManager towerManager { get; set; }
        private CameraManager _cameraManager;
        private Camera _cam;
        private Transform _cursorChild;
        private Vector3 _worldGridPos;
        private TowerType _selectedTowerType;
        private bool _isUnitTower;
        private bool _canPlace;
        private bool _startPlacement;
        private Vector3[] _checkDir;
        private MeshRenderer _cursorMeshRenderer;
        private RaycastHit _unitSpawnRay;

        private ParticleSystem cancelPlaceParticle;

        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask towerLayer;
        [SerializeField] private Color[] cubeColor;

        private void Awake()
        {
            _cameraManager = FindObjectOfType<CameraManager>();
            _cam = Camera.main;
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
            cubeCursor.position = Vector3.zero + Vector3.down * 5;
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
            if (_canPlace)
            {
                PlaceTower().Forget();
                _selectedTowerType = TowerType.None;
            }

            StopPlacement();
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
            _startPlacement = false;
            _canPlace = false;
            _cameraManager.enabled = true;
            _cursorMeshRenderer.enabled = false;
            _worldGridPos = Vector3.zero + Vector3.down * 5;
            cubeCursor.position = _worldGridPos;
        }

        public void StartPlacement(in TowerType towerType, bool isUnitTower)
        {
            _startPlacement = true;
            towerManager.OffUI();
            _cameraManager.enabled = false;
            _cursorMeshRenderer.enabled = true;
            if (!towerManager.IsEnoughGold(in towerType)) return;
            if (_selectedTowerType == towerType) return;
            _isUnitTower = isUnitTower;
            _selectedTowerType = towerType;
        }

        private void UpdateCursorPosition()
        {
            var mousePos = Input.mousePosition;
            mousePos.z = _cam.nearClipPlane;
            var ray = _cam.ScreenPointToRay(mousePos);
            var isGround = Physics.Raycast(ray, out var hit, 100, groundLayer);
            if (isGround)
            {
                var cellGridPos = grid.WorldToCell(hit.point);
                _worldGridPos = grid.CellToWorld(cellGridPos);
                cubeCursor.position = _worldGridPos;
            }

            _cursorMeshRenderer.enabled = isGround;
        }

        private void CheckCanPlace()
        {
            _canPlace = _startPlacement && _cursorMeshRenderer.enabled
                                        && CheckPlacementTile()
                                        && (!_isUnitTower || CheckPlaceUnitTower());

            _cursorMeshRenderer.sharedMaterial.color = _canPlace ? cubeColor[0] : cubeColor[1];
        }

        private bool CheckPlacementTile()
        {
            return Physics.Raycast(_cursorChild.position + Vector3.up * 5, Vector3.down, out _unitSpawnRay, 10) &&
                   _unitSpawnRay.collider.CompareTag("Placement") &&
                   !Physics.CheckSphere(_cursorChild.position, 0.2f, towerLayer);
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

        private async UniTaskVoid PlaceTower()
        {
            _worldGridPos = _cursorChild.position;
            _worldGridPos.y = 1f;
            await towerManager.InstantiateTower(_selectedTowerType, _worldGridPos);
            if (_isUnitTower) towerManager.SetUnitPosition(_unitSpawnRay.point);
            towerManager.BuildTower();
        }
    }
}