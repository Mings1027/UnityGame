using BuildControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using ToolTipControl;
using TowerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManagerControl
{
    public sealed class UIManager : Singleton<UIManager>
    {
        private Tower _curSelectedTower;
        private Transform _uiTarget;
        private Transform _buildingPoint;

        private Vector3 _buildPos;
        private Quaternion _buildRot;
        private int _towerIndex;

        private int _uniqueLevel;

        private bool _isSell, _isTower, _panelIsOpen;
        private bool _isTowerPanel, isEditPanel;
        private bool _isPause;

        public bool IsMoveUnit { get; private set; }
        private bool IsPause { get; set; }

//===============================================Init==============================================
        private MapController _mapController;
        private EventSystem _eventSystem;
        private Camera _cam;
        private MeshRenderer _towerRangeIndicator;
        private GameObject _moveUnitIndicator;
        private MeshFilter _curTowerMesh;
        private Mesh[] _towerMesh;

//=============================================GamePlay UI=================================================================
        private Transform _towerPanels;
        private GameObject _towerSelectPanel;
        private GameObject _towerEditPanel;
        private GameObject _upgradeButton;
        private GameObject _aUpgradeButton;
        private GameObject _bUpgradeButton;
        private GameObject _sellButton;
        private GameObject _moveUnitButton;

        private ToolTipSystem _tooltip;
        private GameObject _okButton;

//================================================UI================================================================        
        private GameObject _menuPanel;
        private GameObject _startGameButton;
        private GameObject _pauseButton;

        private string[] _towerNames;
        private Sequence _towerSelectPanelSequence;
        private Tween _towerEditPanelTween;
        private Button[] _towerSelectButtons;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;
        [SerializeField] private float speed;

        private void Awake()
        {
            Init();
        }

        private void LateUpdate()
        {
            if (!_panelIsOpen || _uiTarget == _towerPanels) return;
            var targetPos = _cam.WorldToScreenPoint(_uiTarget.position);
            _towerPanels.position = Vector3.Lerp(_towerPanels.position, targetPos, Time.deltaTime * speed);
        }

        #region Initalize

        private void Init()
        {
//======================================================Init==============================================================            
            _mapController = MapController.Instance;
            _eventSystem = EventSystem.current;
            _cam = Camera.main;
            _towerRangeIndicator = transform.Find("TowerRangeIndicator").GetComponent<MeshRenderer>();
            _moveUnitIndicator = transform.Find("MoveUnitIndicator").gameObject;
            _curTowerMesh = transform.Find("CurTowerMesh").GetComponent<MeshFilter>();

            _towerMesh = new Mesh[4];
            for (var i = 0; i < _towerMesh.Length; i++)
            {
                _towerMesh[i] = towerLevelManagers[i].towerLevels[0].towerMesh.sharedMesh;
            }

//======================================================= GamePlay UI ==================================================================
            var gamePlayUI = transform.Find("GamePlay UI");

            _towerPanels = gamePlayUI.Find("TowerPanels");
            _towerSelectPanel = _towerPanels.Find("TowerSelectPanel").gameObject;
            _towerEditPanel = _towerPanels.Find("TowerEditPanel").gameObject;

            _upgradeButton = _towerEditPanel.transform.Find("UpgradeButton").gameObject;
            _upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);

            _aUpgradeButton = _towerEditPanel.transform.Find("A UpgradeButton").gameObject;
            _aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(3));

            _bUpgradeButton = _towerEditPanel.transform.Find("B UpgradeButton").gameObject;
            _bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(4));

            _sellButton = _towerEditPanel.transform.Find("SellButton").gameObject;
            _sellButton.GetComponent<Button>().onClick.AddListener(SellButton);

            _moveUnitButton = _towerEditPanel.transform.Find("MoveUnitButton").gameObject;
            _moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            _tooltip = _towerPanels.Find("TooltipCanvas").GetComponent<ToolTipSystem>();

            _okButton = _towerPanels.Find("OKButton").gameObject;
            _okButton.GetComponent<Button>().onClick.AddListener(OkButton);

//============================================================ UI ========================================================================            
            var ui = transform.Find("UI");

            _menuPanel = ui.Find("MenuPanel").gameObject;
            _startGameButton = ui.Find("StartGameButton").gameObject;
            _startGameButton.GetComponent<Button>().onClick.AddListener(StartGameButton);
            _pauseButton = ui.Find("PauseButton").gameObject;
            _pauseButton.GetComponent<Button>().onClick.AddListener(Pause);

            _towerNames = new string[_towerSelectPanel.transform.childCount];
            for (var i = 0; i < _towerNames.Length; i++)
            {
                _towerNames[i] = _towerSelectPanel.transform.GetChild(i).name
                    .Replace("Button", "");
            }

            _towerSelectPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            _towerEditPanelTween = _towerEditPanel.transform.DOScale(1, 0.05f).From(0).SetAutoKill(false);

            _towerSelectButtons = new Button[_towerSelectPanel.transform.childCount];
            for (var i = 0; i < _towerSelectButtons.Length; i++)
            {
                var index = i;
                _towerSelectButtons[i] = _towerSelectPanel.transform.GetChild(i).GetComponent<Button>();
                _towerSelectButtons[i].onClick.AddListener(() => TowerButton(index));

                _towerSelectPanelSequence.Append(_towerSelectButtons[i].transform.DOScale(1, 0.05f).From(0))
                    .Join(_towerSelectButtons[i].transform.DOLocalMove(Vector3.zero, 0.05f).From());
            }

            _towerSelectPanel.SetActive(false);
            _towerEditPanel.SetActive(false);
            _okButton.SetActive(false);
        }

        #endregion

        private void StartGameButton()
        {
            _mapController.BuildPointSequence.Restart();
            _startGameButton.SetActive(false);
        }

        private void Pause()
        {
            IsPause = !IsPause;
            Time.timeScale = IsPause ? 0 : 1;
            _menuPanel.SetActive(IsPause);
        }

        #region Tower Select Panel

        public void OpenTowerSelectPanel(Transform t)
        {
            CloseUI();

            _isTowerPanel = true;
            _panelIsOpen = true;
            _isTower = false;
            _buildPos = t.position;
            _buildRot = t.rotation;
            _buildingPoint = t;
            _uiTarget = _buildingPoint;
            // _towerPanels.position = _buildingPoint.position;
            _towerSelectPanel.SetActive(true);
            _towerEditPanel.SetActive(false);

            _towerSelectPanelSequence.Restart();
        }

        private void TowerButton(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            _curTowerMesh.transform.SetPositionAndRotation(_buildPos, _buildRot);
            _curTowerMesh.sharedMesh = tempTowerLevel.towerMesh.sharedMesh;

            _towerIndex = index;
            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);

            var indicatorTransform = _towerRangeIndicator.transform;
            indicatorTransform.position = _curTowerMesh.transform.position;
            indicatorTransform.localScale =
                new Vector3(tempTowerLevel.attackRange * 2, 0.1f, tempTowerLevel.attackRange * 2);

            if (_towerRangeIndicator.enabled) return;
            _towerRangeIndicator.enabled = true;
        }

        #endregion

        #region Tower Edit Panel

        private void OpenTowerEditPanel(Tower t, Transform uiTarget)
        {
            CloseUI();
            _curSelectedTower = t;
            isEditPanel = true;
            _panelIsOpen = true;
            _isTower = true;

            _moveUnitButton.SetActive(_curSelectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = _towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);

            if (!_towerRangeIndicator.enabled)
                _towerRangeIndicator.enabled = true;

            switch (t.TowerLevel)
            {
                case >= 3:
                    _upgradeButton.SetActive(false);
                    _aUpgradeButton.SetActive(false);
                    _bUpgradeButton.SetActive(false);
                    break;
                case 2:
                    _upgradeButton.SetActive(false);
                    _aUpgradeButton.SetActive(true);
                    _bUpgradeButton.SetActive(true);
                    break;
                default:
                    _upgradeButton.SetActive(true);
                    _aUpgradeButton.SetActive(false);
                    _bUpgradeButton.SetActive(false);
                    break;
            }

            _uiTarget = uiTarget;
            // _towerPanels.position = uiTarget.position;

            _towerEditPanel.SetActive(true);
            _towerSelectPanel.SetActive(false);

            _towerEditPanelTween.Restart();
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
            IsMoveUnit = true;
            _moveUnitButton.SetActive(false);
            CloseUI();
            var moveUnitIndicatorTransform = _moveUnitIndicator.transform;
            moveUnitIndicatorTransform.position = _curSelectedTower.transform.position;
            moveUnitIndicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);
            _moveUnitIndicator.SetActive(true);
        }

        #endregion

        #region Init UI

        public void CloseUI()
        {
            if (!_panelIsOpen) return;

            _panelIsOpen = false;
            if (_isTowerPanel)
            {
                _isTowerPanel = false;
                _towerSelectPanelSequence.PlayBackwards();
            }

            if (isEditPanel)
            {
                isEditPanel = false;
                _towerEditPanelTween.PlayBackwards();
            }

            if (_tooltip.gameObject.activeSelf) _tooltip.Hide();

            if (_okButton.activeSelf) _okButton.SetActive(false);

            if (_towerRangeIndicator.enabled) _towerRangeIndicator.enabled = false;

            if (_moveUnitIndicator.activeSelf) _moveUnitIndicator.SetActive(false);

            if (_curTowerMesh.sharedMesh == null) return;
            _curTowerMesh.sharedMesh = null;
        }

        #endregion

        private void ActiveOkButton(string info, string towerName)
        {
            _tooltip.Show(_towerSelectPanel.transform.position, info, towerName);
            _okButton.transform.position = _isTower
                ? _eventSystem.currentSelectedGameObject.transform.position
                : _towerSelectButtons[_towerIndex].transform.position;

            _okButton.SetActive(true);
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
                    TowerUpgrade().Forget();
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
            StackObjectPool.Get("BuildSmoke", _curSelectedTower.transform);
            StackObjectPool.Get("BuildingPoint", _curSelectedTower.transform);
        }

        private void TowerBuild()
        {
            _curSelectedTower = StackObjectPool.Get<Tower>(_towerNames[_towerIndex], _buildPos, _buildRot);
            _buildingPoint.gameObject.SetActive(false);
            TowerUpgrade().Forget();

            _curSelectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
        }

        public void MoveUnit()
        {
            if (_curSelectedTower.GetComponent<BarracksUnitTower>().Move())
            {
                IsMoveUnit = false;
            }
            else
            {
                print("Can't Move");
                // X표시 UI를 나타나게 해준다던가 이펙트표시해주면 좋을듯
            }
        }

        private async UniTaskVoid TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            tempTower.TowerLevelUp(_uniqueLevel);
            StackObjectPool.Get("BuildSmoke", tempTower.transform.position);
            var tlm = towerLevelManagers[(int)tempTower.TowerType];
            var tl = tlm.towerLevels[tempTower.TowerLevel];

            tempTower.TowerInit(tl.consMesh);

            if (tempTower.TowerType == Tower.Type.Barracks)
            {
                tempTower.GetComponent<BarracksUnitTower>().UnitHealth = tl.health;
            }

            await UniTask.Delay(1000);

            tempTower.TowerSetting(tl.towerMesh, tl.minDamage, tl.maxDamage, tl.attackRange,
                tl.attackDelay);
        }
    }
}