using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using MapControl;
using ToolTipControl;
using TowerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class GamePlayUIController : MonoBehaviour
    {
        private GameManager _gameManager;
        private Camera _cam;
        private EventSystem _eventSystem;
        private Tween _towerSelectPanelTween;
        private Tween _towerEditPanelTween;

        private Tower _curSelectedTower;

        private Transform _buildTransform;
        private Transform _uiTarget;
        private Transform _okButtonTarget;
        private Transform _toolTipTarget;

        private string _towerTypeName;

        private int _uniqueLevel;
        private int _sellTowerCoin;

        private bool _panelIsOpen;
        private bool _isSell;
        private bool _isTower;
        private bool _isTowerPanel, _isEditPanel;
        private bool _isMoveUnit;

        private Dictionary<string, TowerData> _towerDictionary;

        [SerializeField] private UIManager uiManager;

        [SerializeField] private Button startButton;

        [SerializeField] private ToolTipSystem tooltip;

        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton;
        [SerializeField] private GameObject bUpgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private GameObject okButton;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer curTowerMeshRenderer;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private GameObject moveUnitIndicator;

        [SerializeField] private TowerData[] towerData;

        [SerializeField] private int[] towerBuildCoin;

        /*======================================================================================================================
         *                                        Unity Event
         ======================================================================================================================*/
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            startButton.onClick.AddListener(StartGame);

            upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(0));
            bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(1));
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            moveUnitIndicator.GetComponent<MoveUnitIndicator>().onMoveUnitEvent += MoveUnit;
        }

        private void Start()
        {
            Init();
            TowerDataInit();
            startPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.SetActive(false);
        }

        private void LateUpdate()
        {
            MoveUI();
            if (okButton.activeSelf)
            {
                okButton.transform.position = _okButtonTarget.position;
            }
        }

        private void OnDestroy()
        {
            _towerSelectPanelTween.Kill();
            _towerEditPanelTween.Kill();
        }

        /*======================================================================================================================
         *                                   Init
         ======================================================================================================================*/
        private void Init()
        {
            Time.timeScale = 0;

            var towerButtons = new Button[towerSelectPanel.transform.childCount];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = towerSelectPanel.transform.GetChild(i).gameObject.GetComponent<Button>();
                var t = towerButtons[i].GetComponent<TowerButton>();
                towerButtons[i].onClick.AddListener(() => TowerSelectButtons(t.TowerTypeName));
            }

            _towerSelectPanelTween = towerSelectPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
            _towerEditPanelTween = towerEditPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);

            GameManager.Instance.Cam.onCloseUIEvent += CloseUI;
        }

        private void TowerDataInit()
        {
            _towerDictionary = new Dictionary<string, TowerData>
            {
                { "ArcherTower", towerData[0] },
                { "BarracksTower", towerData[1] },
                { "CanonTower", towerData[2] },
                { "MageTower", towerData[3] }
            };
        }

        private void StartGame()
        {
            Time.timeScale = 1;
            startPanel.transform.DOMoveY(Screen.height * 3, 1f).SetEase(Ease.InBack)
                .OnComplete(() => startPanel.SetActive(false));
        }

        private void MoveUI()
        {
            if (!_panelIsOpen) return;
            if (_isTowerPanel)
            {
                MoveUIPlease(towerSelectPanel.transform);
            }

            if (_isEditPanel)
            {
                MoveUIPlease(towerEditPanel.transform);
            }
        }

        private void MoveUIPlease(Transform ui)
        {
            var targetPos = _cam.WorldToScreenPoint(_uiTarget.position);
            ui.position = Vector3.Lerp(ui.position, targetPos, 1);
        }

        public void OpenTowerSelectPanel(Transform t)
        {
            CloseUI();
            _isTower = false;
            _panelIsOpen = true;
            _isTowerPanel = true;
            _uiTarget = t;
            _toolTipTarget = towerSelectPanel.transform;
            if (!towerSelectPanel.activeSelf) towerSelectPanel.SetActive(true);
            _buildTransform = t;
            _towerSelectPanelTween.Restart();
        }

        private void OpenTowerEditPanel(Tower t)
        {
            CloseUI();
            if (!towerEditPanel.activeSelf)
                towerEditPanel.SetActive(true);
            _towerEditPanelTween.Restart();
            _isTower = true;
            _panelIsOpen = true;
            _isEditPanel = true;
            _curSelectedTower = t;
            _uiTarget = _curSelectedTower.transform;
            _toolTipTarget = towerEditPanel.transform;

            moveUnitButton.SetActive(_curSelectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);

            if (!towerRangeIndicator.enabled)
                towerRangeIndicator.enabled = true;

            upgradeButton.SetActive(t.TowerLevel != 2);
            aUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
            bUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
        }

        private void MoveUnitButton()
        {
            _isMoveUnit = true;
            moveUnitButton.SetActive(false);
            CloseUI();
            var moveUnitIndicatorTransform = moveUnitIndicator.transform;
            moveUnitIndicatorTransform.position = _curSelectedTower.transform.position;
            moveUnitIndicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);
            moveUnitIndicator.SetActive(true);
        }

        private void MoveUnit()
        {
            if (_curSelectedTower.GetComponent<BarracksUnitTower>().Move())
            {
                _isMoveUnit = false;
            }
            else
            {
                print("Can't Move");
                // X표시 UI를 나타나게 해준다던가 이펙트표시해주면 좋을듯
            }
        }

        private void CloseUI()
        {
            if (!_panelIsOpen) return;
            _gameManager.IsClickBuildPoint = false;
            _panelIsOpen = false;
            _isSell = false;

            if (_isTowerPanel)
            {
                _isTowerPanel = false;
                _towerSelectPanelTween.PlayBackwards();
            }

            if (_isEditPanel)
            {
                _isEditPanel = false;
                _towerEditPanelTween.PlayBackwards();
            }

            if (tooltip.gameObject.activeSelf) tooltip.Hide();

            if (okButton.activeSelf) okButton.SetActive(false);

            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;

            if (moveUnitIndicator.activeSelf) moveUnitIndicator.SetActive(false);

            curTowerMeshRenderer.enabled = false;
        }

        private void TowerSelectButtons(string towerName)
        {
            var tempTower = _towerDictionary[towerName].towerLevels[0];
            _towerTypeName = towerName;

            curTowerMesh.transform.SetPositionAndRotation(_buildTransform.position, _buildTransform.rotation);
            curTowerMesh.sharedMesh = tempTower.towerMesh.sharedMesh;
            curTowerMeshRenderer.enabled = true;

            ActiveOkButton(tempTower.towerInfo, tempTower.towerName);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = curTowerMesh.transform.position;
            var range = tempTower.attackRange * 2;
            indicatorTransform.localScale = new Vector3(range, 0.1f, range);

            if (towerRangeIndicator.enabled) return;
            towerRangeIndicator.enabled = true;
        }

        private void TowerBuild()
        {
            _curSelectedTower = StackObjectPool.Get<Tower>(_towerTypeName, _buildTransform);
            _curSelectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
            _buildTransform.gameObject.SetActive(false);
        }

        private async UniTaskVoid TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var t = towerData[(int)tempTower.TowerType];

            TowerData.TowerLevelData tt;

            if (tempTower.TowerLevel != 2)
            {
                tempTower.TowerLevelUp();
                tt = t.towerLevels[tempTower.TowerLevel];
                uiManager.TowerCoin -= towerBuildCoin[tempTower.TowerLevel];
            }
            else
            {
                tempTower.TowerUniqueLevelUp(_uniqueLevel);
                tt = t.towerUniqueLevels[tempTower.TowerUniqueLevel];
                uiManager.TowerCoin -= towerBuildCoin[3];
            }

            StackObjectPool.Get(PoolObjectName.BuildSmoke, tempTower.transform.position);
            tempTower.TowerInit(tt.consMesh);

            await UniTask.Delay(1000);

            tempTower.TowerSetting(tt.towerMesh, tt.minDamage, tt.maxDamage, tt.attackRange,
                tt.attackDelay, tt.health);
        }

        private void UpgradeButton()
        {
            _isSell = false;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType]
                .towerLevels[_curSelectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        private void UniqueUpgradeButton(int index)
        {
            _isSell = false;
            _uniqueLevel = index;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType].towerUniqueLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        private void SellButton()
        {
            _isSell = true;
            _sellTowerCoin = SellTowerCoin();
            ActiveOkButton($"이 타워를 처분하면{_sellTowerCoin.ToString()} 골드가 반환됩니다.", "타워처분");
        }

        private int SellTowerCoin()
        {
            var towerLevel = _curSelectedTower.IsUniqueTower ? 4 : _curSelectedTower.TowerLevel + 1;

            var sum = 0;
            for (var i = 0; i < towerLevel; i++)
            {
                sum += towerBuildCoin[i];
            }

            return sum;
        }

        private void SellTower()
        {
            _curSelectedTower.gameObject.SetActive(false);
            StackObjectPool.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);
            StackObjectPool.Get(PoolObjectName.BuildingPoint, _curSelectedTower.transform);

            uiManager.TowerCoin += _sellTowerCoin;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            tooltip.Show(_toolTipTarget, info, towerName);
            _okButtonTarget = _eventSystem.currentSelectedGameObject.transform;
            okButton.GetComponent<Button>().interactable = CanBuildCheck();
            okButton.SetActive(true);
        }

        private bool CanBuildCheck()
        {
            if (_isSell) return true;
            if (_isTower)
            {
                if (uiManager.TowerCoin >= towerBuildCoin[_curSelectedTower.TowerLevel + 1]) return true;
            }
            else
            {
                if (uiManager.TowerCoin >= 70) return true;
            }

            return false;
        }

        private void OkButton()
        {
            if (_isSell) SellTower();
            else
            {
                if (!_isTower) TowerBuild();
                TowerUpgrade().Forget();
            }

            CloseUI();
        }
    }
}