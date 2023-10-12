using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class InputManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private Camera _cam;
        private Transform _cursorChild;
        private Vector3 _worldGridPos;
        private TowerType _selectedTowerType;
        private bool _isUnitTower;
        private bool isGround;
        private bool _canPlace;
        private bool _startPlacement;
        private Vector3[] _checkDir;
        private MeshRenderer _cursorMeshRenderer;
        private RaycastHit _hit;

        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Color[] cubeColor;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _cam = Camera.main;
            _cursorMeshRenderer = cubeCursor.GetComponentInChildren<MeshRenderer>();
            _cursorChild = cubeCursor.GetChild(0);

            _checkDir = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                new Vector3(1, 0, 1), new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1), new Vector3(1, 0, -1)
            };
            for (var i = 0; i < _checkDir.Length; i++)
            {
                _checkDir[i] = _checkDir[i] * 2 + Vector3.up;
            }

            _selectedTowerType = TowerType.None;
            cubeCursor.position = Vector3.zero + Vector3.down * 5;
        }

        private void Update()
        {
            if (!_startPlacement) return;

            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase.Equals(TouchPhase.Began) || touch.phase.Equals(TouchPhase.Moved) ||
                touch.phase.Equals(TouchPhase.Stationary))
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
                PlaceTower();
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
            for (var i = 0; i < _checkDir.Length; i++)
            {
                Gizmos.DrawRay(_cursorChild.position + _checkDir[i], Vector3.down * 10);
            }
        }
#endif
        private void StopPlacement()
        {
            _startPlacement = false;
            _canPlace = false;
            _gameManager.towerManager.enabled = true;
            _worldGridPos = Vector3.zero + Vector3.down * 5;

            _cursorMeshRenderer.transform.DOScale(0, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                cubeCursor.position = _worldGridPos;
            });
        }

        public void StartPlacement(in TowerType towerType, bool isUnitTower)
        {
            _startPlacement = true;
            _gameManager.towerManager.OffUI();
            _gameManager.cameraManager.enabled = false;
            _cursorMeshRenderer.transform.DOScale(2, 0.5f).SetEase(Ease.OutBack);
            if (!_gameManager.towerManager.IsEnoughGold(in towerType)) return;
            if (_selectedTowerType == towerType) return;
            _isUnitTower = isUnitTower;
            _selectedTowerType = towerType;
        }

        private void UpdateCursorPosition()
        {
            var mousePos = Input.mousePosition;
            mousePos.z = _cam.nearClipPlane;
            var ray = _cam.ScreenPointToRay(mousePos);
            isGround = Physics.Raycast(ray, out var hit, 100, groundLayer);
            if (!isGround) return;

            var cellGridPos = grid.WorldToCell(hit.point);
            _worldGridPos = grid.CellToWorld(cellGridPos);
            cubeCursor.DOMove(_worldGridPos, 0.2f).SetEase(Ease.OutBack);
        }

        private void CheckCanPlace()
        {
            _canPlace = _startPlacement && CheckPlacementTile();

            _cursorMeshRenderer.sharedMaterial.color = _canPlace ? cubeColor[0] : cubeColor[1];
        }

        private bool CheckPlacementTile()
        {
            Physics.Raycast(_cursorChild.position + Vector3.up * 5, Vector3.down, out _hit, 10);
            if (!_hit.collider.CompareTag("Placement")) return false;
            if (!_isUnitTower) return true;
            for (var i = 0; i < _checkDir.Length; i++)
            {
                if (!Physics.Raycast(_cursorChild.position + _checkDir[i], Vector3.down,
                        out _hit, 10)) continue;
                if (_hit.collider.CompareTag("Ground"))
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
            _gameManager.towerManager.InstantiateTower(_selectedTowerType, _worldGridPos, _isUnitTower).Forget();
        }
    }
}