// using DG.Tweening;
// using GameControl;
// using ToolTipControl;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace ManagerControl
// {
//     public class BuildingManager : MonoBehaviour
//     {
//         private Camera _cam;
//
//         private Ray _camRay;
//         private RaycastHit _hit;
//
//         private Vector3 _cursorPos;
//
//         private GameObject _tower;
//         // private GameObject _selectedObject;
//
//         private bool _isBuilding, _isEditMode, _isGridMove;
//
//         public bool canPlace;
//
//         [SerializeField] private InputController input;
//
//         [SerializeField] private LayerMask groundLayer, towerLayer;
//
//         [SerializeField] private GameObject buildModePanel;
//         [SerializeField] private GameObject editModePanel;
//
//         [SerializeField] private GameObject[] towerName;
//         [SerializeField] private Button[] towerButtons;
//
//         [Space(10)] [Header("Camera Tween")] [SerializeField]
//         private float duration;
//
//         [SerializeField] private float strength;
//
//         private void Awake()
//         {
//             _cam = Camera.main;
//             input.OnCursorPositionEvent += CursorPosition;
//
//             input.OnBuildTowerEvent += BuildTower;
//             input.OnClickTowerEvent += ClickTower;
//
//             input.OnCancelBuildEvent += CancelBuildMode;
//             input.OnCancelEditEvent += CancelEditMode;
//
//             towerButtons = new Button[towerName.Length];
//             for (var i = 0; i < towerButtons.Length; i++)
//             {
//                 towerButtons[i] = buildModePanel.transform.GetChild(i).GetComponent<Button>();
//                 var ii = i;
//                 towerButtons[i].onClick.AddListener(() => ActiveBuildMode(ii));
//             }
//         }
//
//         private void CursorPosition(Vector2 cursorPos)
//         {
//             _camRay = _cam.ScreenPointToRay(cursorPos);
//             if (Physics.Raycast(_camRay, out _hit, 1000, groundLayer))
//             {
//                 if (_isGridMove)
//                 {
//                     GridCursor();
//                 }
//                 else
//                 {
//                     _cursorPos = _hit.point;
//                 }
//             }
//
//             if (_tower != null && _isBuilding) _tower.transform.position = _cursorPos;
//         }
//
//         private void GridCursor()
//         {
//             _cursorPos.x = Mathf.Round(_hit.point.x);
//             _cursorPos.y = Mathf.Round(_hit.point.y);
//             _cursorPos.z = Mathf.Round(_hit.point.z);
//         }
//
//         private void ActiveBuildMode(int index)
//         {
//             if (_isEditMode)
//             {
//                 _isEditMode = false;
//                 DeSelect();
//                 _tower = null;
//             }
//
//             if (_tower == null)
//             {
//                 _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
//             }
//             else
//             {
//                 _tower.gameObject.SetActive(false);
//                 _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
//             }
//
//             if (_isBuilding) return;
//             _isBuilding = true;
//             input.isBuild = true;
//         }
//
//         private void BuildTower()
//         {
//             if (!canPlace || _tower == null) return;
//             _isBuilding = false;
//             input.isBuild = false;
//             _tower = null;
//             _cam.DOShakePosition(duration, strength, randomness: 180);
//         }
//
//         private void ClickTower()
//         {
//             if (!Physics.Raycast(_camRay, out var t, 1000, towerLayer)) return;
//             ActiveEditMode();
//             OutLineTower(t.collider.gameObject);
//         }
//
//         private void CancelBuildMode()
//         {
//             if (_isBuilding) _isBuilding = false;
//             if (_tower != null) _tower.SetActive(false);
//         }
//
//         private void ActiveEditMode()
//         {
//             if (_isEditMode) return;
//             _isEditMode = true;
//             input.isEdit = true;
//         }
//
//         private void CancelEditMode()
//         {
//             if (_isEditMode) _isEditMode = false;
//             input.isEdit = false;
//             DeSelect();
//         }
//
//         private void OutLineTower(GameObject t)
//         {
//             if (t == _tower) return;
//             if (_tower != null) DeSelect();
//             var outLine = t.GetComponent<Outline>();
//             if (outLine == null) t.AddComponent<Outline>();
//             else outLine.enabled = true;
//             _tower = t;
//             if (!editModePanel.activeSelf) editModePanel.SetActive(true);
//             editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
//         }
//
//         private void DeSelect()
//         {
//             _tower.GetComponent<Outline>().enabled = false;
//             _tower = null;
//             if (editModePanel.activeSelf) editModePanel.SetActive(false);
//         }
//     }
// }