using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using ToolTipControl;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private Tower _selectedTower;
        private int _towerIndex;
        private int _uniqueLevel;
        private Transform _buildingPoint;
        private EventSystem _eventSystem;

        private Vector3 _buildPos;
        private Quaternion _buildRot;

        private bool _isSell, _isTower, _panelIsOpen;

        public static bool isPause, pointer;

        [SerializeField] private InputManager input;
        [SerializeField] private Transform towerBuildPoints;

        [Header("Tower Panels")] [Space(10)] [SerializeField]
        private FollowWorld towerPanels;

        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private Button[] towerSelectButtons;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject okButton;

        [Header("Tower Edit Panel")] [Space(10)] [SerializeField]
        private GameObject upgradeButton;

        [SerializeField] private GameObject uniqueUpgradeButtons;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private GameObject moveUnitButton;

        [SerializeField] private ToolTipSystem tooltip;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private MeshRenderer moveUnitIndicator;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;
        private string[] _towerNames;
        private Mesh[] _towerMesh;

        private void Start()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            for (var i = 0; i < towerBuildPoints.transform.childCount; i++)
            {
                var child = towerBuildPoints.transform.GetChild(i);
                StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position, child.rotation)
                    .onOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }

            _towerNames = new string[towerSelectPanel.transform.childCount];
            for (var i = 0; i < _towerNames.Length; i++)
            {
                _towerNames[i] = towerSelectPanel.transform.GetChild(i).name
                    .Replace("Button", "");
            }

            _towerMesh = new Mesh[4];
            for (var i = 0; i < _towerMesh.Length; i++)
            {
                _towerMesh[i] = towerLevelManagers[i].towerLevels[0].towerMesh.sharedMesh;
            }

            towerSelectButtons = new Button[towerSelectPanel.transform.childCount];
            for (var i = 0; i < towerSelectButtons.Length; i++)
            {
                var index = i;
                towerSelectButtons[i] = towerSelectPanel.transform.GetChild(i).GetComponent<Button>();
                towerSelectButtons[i].onClick.AddListener(() => TowerSelectButton(index));
            }

            upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            for (var i = 0; i < uniqueUpgradeButtons.transform.childCount; i++)
            {
                var index = i;
                uniqueUpgradeButtons.transform.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() => UniqueUpgradeButton(index + 3));
            }

            sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            okButton.GetComponent<Button>().onClick.AddListener(OkButton);

            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            input.onMoveUnitEvent += MoveUnit;
            input.onClosePanelEvent += CloseUI;

            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.gameObject.SetActive(false);
            towerPanels.WorldTarget(transform);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointer = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointer = false;
        }

        private void MoveUnit()
        {
            var ray = _cam.ScreenPointToRay(input.mousePos);
            if (Physics.Raycast(ray, out var hit))
            {
                if (Vector3.Distance(_selectedTower.transform.position, hit.point) < _selectedTower.TowerRange)
                {
                    if (!hit.collider.CompareTag("Ground")) return;
                    input.isMoveUnit = false;
                    moveUnitIndicator.enabled = false;
                    _selectedTower.GetComponent<BarracksTower>().MoveUnits(hit.point);
                }
                else
                {
                    print("Can't Move");
                }
            }
        }

        private void OpenTowerSelectPanel(Transform t)
        {
            ResetUI();

            if (!input.selectTower) input.selectTower = true;

            _panelIsOpen = true;
            _isTower = false;
            _buildPos = t.position;
            _buildRot = t.rotation;
            _buildingPoint = t;
            towerPanels.WorldTarget(_buildingPoint);
            _selectedTower = t.GetComponent<Tower>();
            towerSelectPanel.SetActive(true);
            towerEditPanel.SetActive(false);
            towerPanels.ActivePanel();
        }

        public void TowerSelectButton(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            curTowerMesh.transform.SetPositionAndRotation(_buildPos, _buildRot);
            curTowerMesh.sharedMesh = tempTowerLevel.towerMesh.sharedMesh;

            _towerIndex = index;
            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = curTowerMesh.transform.position;
            indicatorTransform.localScale =
                new Vector3(tempTowerLevel.attackRange * 2, 0.1f, tempTowerLevel.attackRange * 2);

            if (towerRangeIndicator.enabled) return;
            towerRangeIndicator.enabled = true;
        }

        private void OpenTowerEditPanel(Tower t, Transform trans)
        {
            ResetUI();
            _selectedTower = t;
            _panelIsOpen = true;
            _isTower = true;

            moveUnitButton.SetActive(_selectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _selectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);

            if (!towerRangeIndicator.enabled)
                towerRangeIndicator.enabled = true;

            switch (t.TowerLevel)
            {
                case >= 3:
                    upgradeButton.SetActive(false);
                    uniqueUpgradeButtons.SetActive(false);
                    break;
                case 2:
                    upgradeButton.SetActive(false);
                    uniqueUpgradeButtons.SetActive(true);
                    break;
                default:
                    upgradeButton.SetActive(true);
                    uniqueUpgradeButtons.SetActive(false);
                    break;
            }

            towerPanels.WorldTarget(trans);
            towerEditPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerPanels.ActivePanel();
        }

        private void CloseUI()
        {
            if (!_panelIsOpen) return;

            input.selectTower = false;
            _panelIsOpen = false;
            ResetUI();
        }

        private void ResetUI()
        {
            input.LastIndex = -1;
            
            if (tooltip.gameObject.activeSelf)
            {
                tooltip.Hide();
            }

            if (okButton.gameObject.activeSelf)
            {
                okButton.SetActive(false);
            }

            if (towerRangeIndicator.enabled)
            {
                towerRangeIndicator.enabled = false;
            }

            if (moveUnitIndicator.enabled)
            {
                moveUnitIndicator.enabled = false;
            }

            towerPanels.DeActivePanel();
            if (curTowerMesh.sharedMesh == null) return;
            curTowerMesh.sharedMesh = null;
        }


        private void UpgradeButton()
        {
            _isSell = false;
            var towerLevel = towerLevelManagers[(int)_selectedTower.TowerType]
                .towerLevels[_selectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        //index should be 3 or 4 Cuz It's Unique Tower Upgrade 
        private void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_selectedTower.TowerType].towerLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        public void OkButton()
        {
            if (_isTower)
            {
                if (_isSell)
                {
                    SellTower();
                }
                else
                {
                    TowerUpgrade(_uniqueLevel, _selectedTower).Forget();
                }
            }
            else
            {
                TowerBuild();
            }

            CloseUI();
        }

        private void SellButton()
        {
            _isSell = true;
            var coin = towerLevelManagers[(int)_selectedTower.TowerType].towerLevels[_selectedTower.TowerLevel].coin;
            ActiveOkButton($"이 타워를 처분하면 {coin} 골드가 반환됩니다.", "타워처분");
        }

        private void SellTower()
        {
            _isSell = false;
            _selectedTower.gameObject.SetActive(false);
            var towerTransform = _selectedTower.transform;
            var position = towerTransform.position;

            StackObjectPool.Get("BuildSmoke", position);
            StackObjectPool.Get("BuildingPoint", position, towerTransform.rotation);
        }

        private void TowerBuild()
        {
            _selectedTower = StackObjectPool.Get<Tower>(_towerNames[_towerIndex], _buildPos, _buildRot);
            _buildingPoint.gameObject.SetActive(false);
            TowerUpgrade(0, _selectedTower).Forget();

            _selectedTower.onResetMeshEvent += ResetMesh;
            _selectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
        }

        private void ResetMesh(MeshFilter meshFilter)
        {
            meshFilter.sharedMesh = towerLevelManagers[_towerIndex].towerLevels[0].towerMesh.sharedMesh;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            tooltip.Show(towerSelectPanel.transform.position, info, towerName);
            if (_eventSystem.currentSelectedGameObject != null)
            {
                okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            }
            else
            {
                okButton.transform.position = towerSelectButtons[_towerIndex].transform.position;
            }

            okButton.SetActive(true);
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            input.isMoveUnit = true;
            ResetUI();
            var moveUnitIndicatorTransform = moveUnitIndicator.transform;
            moveUnitIndicatorTransform.position = _selectedTower.transform.position;
            moveUnitIndicatorTransform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);
            moveUnitIndicator.enabled = true;
        }

        private async UniTaskVoid TowerUpgrade(int level, Tower selectedTower)
        {
            selectedTower.TowerLevelUp(level);
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position);
            var t = towerLevelManagers[(int)selectedTower.TowerType];
            var tt = t.towerLevels[selectedTower.TowerLevel];
            selectedTower.UnderConstruction(tt.consMesh);

            if (selectedTower.TowerType == Tower.Type.Barracks)
            {
                _selectedTower.GetComponent<BarracksTower>().UnitHealth = tt.health;
            }

            await UniTask.Delay(1000);

            selectedTower.ConstructionFinished(tt.towerMesh, tt.minDamage, tt.maxDamage, tt.attackRange,
                tt.attackDelay);
        }
    }
}