using BuildControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using ManagerControl;
using ToolTipControl;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public sealed class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Tower _curSelectedTower;
        private Transform _uiTarget;
        private Transform _buildingPoint;

        private Vector3 _buildPos;
        private Quaternion _buildRot;
        private int _towerIndex;
        private int _uniqueLevel;
        private int _lastIndex;

        private bool _isPressTowerButton;
        private bool _isSell, _isTower, _panelIsOpen;
        private bool isPause;
        private bool _isOpenSelectPanel;

//===================================Init==============================================
        private Sequence _towerSelectPanelSequence, _towerEditPanelSequence;
        private Tweener _moveTowerPanelTween;
        private BuildingPointController buildingPointController;

        private EventSystem eventSystem;
        private Camera _cam;
        private MeshRenderer towerRangeIndicator;
        private MeshRenderer moveUnitIndicator;
        private MeshFilter curTowerMesh;
        private Mesh[] _towerMesh;

        private Transform ui;
        private Transform towerPanels;
        private GameObject startGameButton;

        private GameObject towerSelectPanel;
        private GameObject towerEditPanel;
        private ToolTipSystem tooltip;
        private GameObject okButton;

        private string[] _towerNames;
        private Button[] towerSelectButtons;
        private Button[] towerEditButtons;

        private GameObject upgradeButton;
        private GameObject aUpgradeButton;
        private GameObject bUpgradeButton;
        private GameObject sellButton;
        private GameObject moveUnitButton;

        public static bool IsOnUI { get; private set; }

        [SerializeField] private InputManager input;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (!_panelIsOpen || _uiTarget == towerPanels) return;
            _moveTowerPanelTween.ChangeEndValue(_cam.WorldToScreenPoint(_uiTarget.position), true).Restart();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsOnUI = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsOnUI = false;
        }

        #region Initalize

        private void Init()
        {
            buildingPointController = BuildingPointController.Instance;
            eventSystem = EventSystem.current;
            _cam = Camera.main;
            towerRangeIndicator = transform.Find("TowerRangeIndicator").GetComponent<MeshRenderer>();
            moveUnitIndicator = transform.Find("MoveUnitIndicator").GetComponent<MeshRenderer>();
            curTowerMesh = transform.Find("CurTowerMesh").GetComponent<MeshFilter>();

            _towerMesh = new Mesh[4];
            for (var i = 0; i < _towerMesh.Length; i++)
            {
                _towerMesh[i] = towerLevelManagers[i].towerLevels[0].towerMesh.sharedMesh;
            }

            ui = transform.GetChild(0);
            towerPanels = ui.Find("TowerPanels");
            startGameButton = ui.Find("StartGameButton").gameObject;

            towerSelectPanel = towerPanels.Find("TowerSelectPanel").gameObject;
            towerEditPanel = towerPanels.Find("TowerEditPanel").gameObject;
            tooltip = towerPanels.Find("TooltipCanvas").GetComponent<ToolTipSystem>();
            okButton = towerPanels.Find("OKButton").gameObject;

            _towerNames = new string[towerSelectPanel.transform.childCount];
            for (var i = 0; i < _towerNames.Length; i++)
            {
                _towerNames[i] = towerSelectPanel.transform.GetChild(i).name
                    .Replace("Button", "");
            }

            _towerSelectPanelSequence = DOTween.Sequence().SetAutoKill(false);
            _moveTowerPanelTween = towerPanels.transform.DOMove(transform.position, 0f).SetAutoKill(false);

            towerSelectButtons = new Button[towerSelectPanel.transform.childCount];
            for (var i = 0; i < towerSelectButtons.Length; i++)
            {
                var index = i;
                towerSelectButtons[i] = towerSelectPanel.transform.GetChild(i).GetComponent<Button>();
                towerSelectButtons[i].onClick.AddListener(() => TowerSelectButton(index));

                _towerSelectPanelSequence.Append(towerSelectButtons[i].transform.DOScale(1, 0.1f).From(0))
                    .Join(towerSelectButtons[i].transform.DOLocalMove(Vector3.zero, 0.1f).From());
            }

            _towerSelectPanelSequence.Pause();

            _towerEditPanelSequence = DOTween.Sequence().SetAutoKill(false);
            towerEditButtons = new Button[towerEditPanel.transform.childCount];
            for (var i = 0; i < towerEditButtons.Length; i++)
            {
                towerEditButtons[i] = towerEditPanel.transform.GetChild(i).GetComponent<Button>();

                _towerEditPanelSequence.Append(towerEditButtons[i].transform.DOScale(1, 0.05f).From(0))
                    .Join(towerEditButtons[i].transform.DOLocalMove(Vector3.zero, 0.1f).From());
            }

            _towerEditPanelSequence.Pause();

            startGameButton.GetComponent<Button>().onClick.AddListener(StartGameButton);
            upgradeButton = towerEditPanel.transform.Find("UpgradeButton").gameObject;
            aUpgradeButton = towerEditPanel.transform.Find("A UpgradeButton").gameObject;
            bUpgradeButton = towerEditPanel.transform.Find("B UpgradeButton").gameObject;
            moveUnitButton = towerEditPanel.transform.Find("MoveUnitButton").gameObject;
            sellButton = towerEditPanel.transform.Find("SellButton").gameObject;

            upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(3));
            bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(4));

            sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            input.onMoveUnitEvent += MoveUnit;
            input.onClosePanelEvent += CloseUI;
            input.onSelectTowerButtonEvent += SelectTowerButton;

            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.gameObject.SetActive(false);
        }

        #endregion

        private void StartGameButton()
        {
            buildingPointController.BuildPointSequence.Restart();
            startGameButton.SetActive(false);
        }

        #region Tower Select Panel

        public void OpenTowerSelectPanel(Transform t)
        {
            ResetUI();

            if (!_isPressTowerButton) _isPressTowerButton = true;
            _panelIsOpen = true;
            _isTower = false;
            _buildPos = t.position;
            _buildRot = t.rotation;
            _buildingPoint = t;
            _uiTarget = _buildingPoint;
            _curSelectedTower = t.GetComponent<Tower>();
            towerSelectPanel.SetActive(true);
            towerEditPanel.SetActive(false);
            towerPanels.gameObject.SetActive(true);

            _towerSelectPanelSequence.OnComplete(() => _isOpenSelectPanel = true).Restart();
        }

        private void SelectTowerButton(int index)
        {
            if (!_isOpenSelectPanel) return;
            if (!_isPressTowerButton) return;
            if (_lastIndex != index)
            {
                TowerSelectButton(index);
            }
            else
            {
                OkButton();
                _lastIndex = -1;
            }
        }

        private void TowerSelectButton(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            curTowerMesh.transform.SetPositionAndRotation(_buildPos, _buildRot);
            curTowerMesh.sharedMesh = tempTowerLevel.towerMesh.sharedMesh;

            _towerIndex = index;
            _lastIndex = index;
            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = curTowerMesh.transform.position;
            indicatorTransform.localScale =
                new Vector3(tempTowerLevel.attackRange * 2, 0.1f, tempTowerLevel.attackRange * 2);

            if (towerRangeIndicator.enabled) return;
            towerRangeIndicator.enabled = true;
        }

        #endregion

        #region Tower Edit Panel

        private void OpenTowerEditPanel(Tower t, Transform uiTarget)
        {
            ResetUI();
            _curSelectedTower = t;
            _panelIsOpen = true;
            _isTower = true;

            moveUnitButton.SetActive(_curSelectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);

            if (!towerRangeIndicator.enabled)
                towerRangeIndicator.enabled = true;

            switch (t.TowerLevel)
            {
                case >= 3:
                    upgradeButton.SetActive(false);
                    aUpgradeButton.SetActive(false);
                    bUpgradeButton.SetActive(false);
                    break;
                case 2:
                    upgradeButton.SetActive(false);
                    aUpgradeButton.SetActive(true);
                    bUpgradeButton.SetActive(true);
                    break;
                default:
                    upgradeButton.SetActive(true);
                    aUpgradeButton.SetActive(false);
                    bUpgradeButton.SetActive(false);
                    break;
            }

            _uiTarget = uiTarget;

            towerEditPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerPanels.gameObject.SetActive(true);

            _towerEditPanelSequence.Restart();
        }

        private void UpgradeButton()
        {
            _isSell = false;
            var towerLevel = towerLevelManagers[(int)_curSelectedTower.TowerType]
                .towerLevels[_curSelectedTower.TowerLevel + 1];

            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        //index should be 3 or 4 Cuz It's Unique Tower Upgrade 
        private void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_curSelectedTower.TowerType].towerLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        private void SellButton()
        {
            _isSell = true;
            var coin = towerLevelManagers[(int)_curSelectedTower.TowerType].towerLevels[_curSelectedTower.TowerLevel]
                .coin;
            ActiveOkButton($"이 타워를 처분하면 {coin} 골드가 반환됩니다.", "타워처분");
        }

        private void MoveUnitButton()
        {
            input.IsMoveUnit = true;
            moveUnitButton.SetActive(false);
            ResetUI();
            var moveUnitIndicatorTransform = moveUnitIndicator.transform;
            moveUnitIndicatorTransform.position = _curSelectedTower.transform.position;
            moveUnitIndicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);
            moveUnitIndicator.enabled = true;
        }

        #endregion

        #region Init UI

        private void CloseUI()
        {
            if (!_panelIsOpen) return;

            _isPressTowerButton = false;
            _panelIsOpen = false;
            ResetUI();
        }

        private void ResetUI()
        {
            _lastIndex = -1;
            _isOpenSelectPanel = false;

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

            towerPanels.gameObject.SetActive(false);
            if (curTowerMesh.sharedMesh == null) return;
            curTowerMesh.sharedMesh = null;
        }

        #endregion

        private void ActiveOkButton(string info, string towerName)
        {
            tooltip.Show(towerSelectPanel.transform.position, info, towerName);
            okButton.transform.position = _isTower
                ? eventSystem.currentSelectedGameObject.transform.position
                : towerSelectButtons[_towerIndex].transform.position;

            okButton.SetActive(true);
        }

        private void OkButton()
        {
            if (_isTower)
            {
                if (_isSell)
                {
                    SellTower();
                }
                else
                {
                    TowerUpgrade();
                }
            }
            else
            {
                TowerBuild();
            }

            CloseUI();
        }

        private void SellTower()
        {
            _isSell = false;
            _curSelectedTower.gameObject.SetActive(false);
            var towerTransform = _curSelectedTower.transform;
            var position = towerTransform.position;

            StackObjectPool.Get("BuildSmoke", position);
            StackObjectPool.Get("BuildingPoint", position, towerTransform.rotation);
        }

        private void TowerBuild()
        {
            _curSelectedTower = StackObjectPool.Get<Tower>(_towerNames[_towerIndex], _buildPos, _buildRot);
            _buildingPoint.gameObject.SetActive(false);
            TowerUpgrade();

            _curSelectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
        }

        private void MoveUnit()
        {
            TowerController.MoveBarrackUnit(input, _cam, _curSelectedTower, moveUnitIndicator);
        }

        private void TowerUpgrade()
        {
            TowerController.TowerUpgrade(_uniqueLevel, towerLevelManagers, _curSelectedTower).Forget();
        }
    }
}