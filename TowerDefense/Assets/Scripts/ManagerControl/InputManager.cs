using System.Diagnostics;
using CustomEnumControl;
using DG.Tweening;
using UIControl;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class InputManager : MonoBehaviour
    {
        private Camera _cam;
        private Tween _meshScaleTween;
        private Transform _cursorChild;
        private Vector3 _prevCursorPos;
        private Vector3 _worldGridPos;
        private TowerType _selectedTowerType;
        private bool _isUnitTower;
        private bool _isGround;
        private bool _canPlace;
        private bool _startPlacement;
        private bool _isAppeared;
        private Vector3[] _checkDir;
        private Vector3[] _fourDir;
        private MeshRenderer _cursorMeshRenderer;
        private RaycastHit _hit;

        [SerializeField] private Transform cubeCursor;
        [SerializeField] private Grid grid;
        [SerializeField] private LayerMask groundLayer, unitLayer;
        [SerializeField] private Material cursorMaterial;
        [SerializeField] private Color[] cubeColor;
        private static readonly int Property = Shader.PropertyToID("_Placement_Color");

        protected void Awake()
        {
            _cam = Camera.main;

            _cursorMeshRenderer = cubeCursor.GetComponentInChildren<MeshRenderer>();
            _cursorChild = cubeCursor.GetChild(0);
            _checkDir = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right, new Vector3(1, 0, 1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, -1)
            };
            for (var i = 0; i < _checkDir.Length; i++)
            {
                _checkDir[i] *= 2;
            }

            _fourDir = new[]
            {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right
            };
            for (var i = 0; i < _fourDir.Length; i++)
            {
                _fourDir[i] *= 2;
            }

            _selectedTowerType = TowerType.None;
            cubeCursor.position = Vector3.zero + Vector3.down * 5;
            _cursorMeshRenderer.enabled = false;
            _meshScaleTween = _cursorMeshRenderer.transform.DOScale(2, 0.25f).From(0).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            _cursorMeshRenderer.enabled = true;
        }

        private void Start()
        {
            _prevCursorPos = cubeCursor.position;
        }

        private void Update()
        {
            if (Input.touchCount != 1) return;
            var touch = Input.GetTouch(0);

            if (_startPlacement)
            {
                UpdateCursorPosition();
                CheckCanPlace();
                CursorAppear();
            }

            if (touch.phase.Equals(TouchPhase.Ended))
            {
                _startPlacement = false;
                enabled = false;
            }
        }

        private void OnDisable()
        {
            StopPlacement();
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Input.mousePosition, 0.5f);
            if (!_cursorChild) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_cursorChild.position + Vector3.up * 3, 1);
            Gizmos.DrawWireSphere(_cursorChild.position, 0.2f);
            Gizmos.color = Color.green;
            for (var i = 0; i < _checkDir.Length; i++)
            {
                Gizmos.DrawRay(_cursorChild.position + _checkDir[i], Vector3.down * 10);
            }
        }

        public void TryPlaceTower()
        {
            if (_canPlace && !TowerButton.isOnButton)
            {
                PlaceTower();
            }

            StopPlacement();
        }

        public void StartPlacement(TowerType towerType, bool isUnitTower)
        {
            _startPlacement = true;
            UIManager.instance.OffUI();
            _meshScaleTween.Restart();
            if (!UIManager.instance.IsEnoughGold(towerType))
            {
                _startPlacement = false;
                return;
            }

            if (_selectedTowerType == towerType) return;
            _isUnitTower = isUnitTower;
            _selectedTowerType = towerType;
        }

        private void StopPlacement()
        {
            _startPlacement = false;
            _canPlace = false;
            _worldGridPos = Vector3.zero + Vector3.down * 5;
            _meshScaleTween.OnRewind(() => cubeCursor.position = _worldGridPos).PlayBackwards();
        }

        private void UpdateCursorPosition()
        {
            var mousePos = Input.mousePosition;
            mousePos.z = _cam.nearClipPlane;
            var ray = _cam.ScreenPointToRay(mousePos);
            _isGround = Physics.Raycast(ray, out var hit, 100, groundLayer);
            if (!_isGround) return;

            var cellGridPos = grid.WorldToCell(hit.point);
            _worldGridPos = grid.CellToWorld(cellGridPos);
            _worldGridPos.y = 0;
            cubeCursor.position = _worldGridPos;
        }

        private void CheckCanPlace()
        {
            _canPlace = _isGround && CheckPlacementTile();
            cursorMaterial.SetColor(Property, _canPlace ? cubeColor[0] : cubeColor[1]);
        }

        private void CursorAppear()
        {
            if (_prevCursorPos == cubeCursor.position) return;
            _prevCursorPos = cubeCursor.position;
            if (_isGround)
            {
                if (!_isAppeared)
                {
                    _meshScaleTween.OnComplete(() => { _isAppeared = true; }).Restart();
                }
            }
            else
            {
                if (_isAppeared)
                {
                    _meshScaleTween.OnRewind(() => { _isAppeared = false; }).PlayBackwards();
                }
            }
        }

        private bool CheckPlacementTile()
        {
            Physics.Raycast(_cursorChild.position + Vector3.up * 5, Vector3.down, out _hit, 10);
            if (!_hit.collider || !_hit.collider.CompareTag("Placement")) return false;
            if (!_isUnitTower) return true;
            for (var i = 0; i < _checkDir.Length; i++)
            {
                if (!Physics.Raycast(_cursorChild.position + _checkDir[i] + Vector3.up, Vector3.down,
                        out _hit, 10, ~unitLayer)) continue;
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
            Vector3 towerForward = default;
            var foundGround = false;
            for (var i = 0; i < 4; i++)
            {
                var ray = new Ray(_worldGridPos + _checkDir[i] + Vector3.up, Vector3.down);
                Physics.Raycast(ray, out var hit, 2);
                if (!hit.collider || !hit.collider.CompareTag("Ground")) continue;
                foundGround = true;
                towerForward = -_checkDir[i];
                break;
            }

            if (!foundGround) towerForward = _checkDir[Random.Range(0, 4)];

            UIManager.instance.InstantiateTower(_selectedTowerType, _worldGridPos, towerForward).Forget();
        }
    }
}