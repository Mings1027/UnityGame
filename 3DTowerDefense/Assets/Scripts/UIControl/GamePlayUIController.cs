using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using MapControl;
using TMPro;
using ToolTipControl;
using TowerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class GamePlayUIController : MonoBehaviour
    {
        private Camera _cam;
        private EventSystem _eventSystem;
        private Tween _towerSelectPanelTween;
        private Tween _towerEditPanelTween;

        private Tower _curSelectedTower;

        private Transform _buildTransform;
        private Transform _uiTarget;
        private Transform _okButtonTarget;
        private Transform _toolTipTarget;

        private string towerTypeName;

        private int _uniqueLevel;
        private int _sellTowerCoin;

        private bool _panelIsOpen;
        private bool _isSell;
        private bool _isTower;
        private bool _isTowerPanel, isEditPanel;
        private bool _isMoveUnit;

        private Dictionary<string, TowerData> towerDictionary;

        [SerializeField] private UIManager uiManager;

        [SerializeField] private Button startButton;

        [SerializeField] private ToolTipSystem _tooltip;

        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject _upgradeButton;
        [SerializeField] private GameObject _aUpgradeButton;
        [SerializeField] private GameObject _bUpgradeButton;
        [SerializeField] private GameObject _moveUnitButton;
        [SerializeField] private GameObject _sellButton;
        [SerializeField] private GameObject _okButton;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private GameObject moveUnitIndicator;

        [SerializeField] private TowerData[] towerData;

        [SerializeField] private int[] towerBuildCoin;

        /*======================================================================================================================
         *                                        Unity Event
         ======================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            startButton.onClick.AddListener(StartGame);

            _upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            _aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(0));
            _bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(1));
            _moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            _sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            _okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            moveUnitIndicator.GetComponent<MoveUnitIndicator>().onMoveUnitEvent += MoveUnit;
        }

        private void Start()
        {
            Init();
            TowerDataInit();
            startPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            _okButton.SetActive(false);
        }

        private void LateUpdate()
        {
            MoveUI();
            if (_okButton.activeSelf)
            {
                _okButton.transform.position = _okButtonTarget.position;
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
                towerButtons[i].onClick.AddListener(() => TowerButton(t.TowerTypeName));
            }

            _towerSelectPanelTween = towerSelectPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
            _towerEditPanelTween = towerEditPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);

            GameManager.Instance.Map.GetComponent<MapController>().onCloseUIEvent += CloseUI;
        }

        private void TowerDataInit()
        {
            towerDictionary = new Dictionary<string, TowerData>
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
                var targetPos = _cam.WorldToScreenPoint(_uiTarget.position);
                towerSelectPanel.transform.position = Vector3.Lerp(towerSelectPanel.transform.position, targetPos,
                    Time.deltaTime * 100);
            }

            if (isEditPanel)
            {
                var targetPos = _cam.WorldToScreenPoint(_uiTarget.position);
                towerEditPanel.transform.position = Vector3.Lerp(towerEditPanel.transform.position, targetPos,
                    Time.deltaTime * 100);
            }
        }

        public void OpenTowerSelectPanel(Transform t)
        {
            CloseUI();
            _isTower = false;
            _panelIsOpen = true;
            _isTowerPanel = true;
            _uiTarget = t;
            _toolTipTarget = t;
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
            isEditPanel = true;
            _curSelectedTower = t;
            var towerTransform = t.transform;
            _uiTarget = towerTransform;
            _toolTipTarget = towerTransform;

            _moveUnitButton.SetActive(_curSelectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);

            if (!towerRangeIndicator.enabled)
                towerRangeIndicator.enabled = true;

            _upgradeButton.SetActive(t.TowerLevel != 2);
            _aUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
            _bUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
        }

        private void MoveUnitButton()
        {
            _isMoveUnit = true;
            _moveUnitButton.SetActive(false);
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
            _panelIsOpen = false;
            _isSell = false;

            if (_isTowerPanel)
            {
                _isTowerPanel = false;
                _towerSelectPanelTween.PlayBackwards();
            }

            if (isEditPanel)
            {
                isEditPanel = false;
                _towerEditPanelTween.PlayBackwards();
            }

            if (_tooltip.gameObject.activeSelf) _tooltip.Hide();

            if (_okButton.activeSelf) _okButton.SetActive(false);

            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;

            if (moveUnitIndicator.activeSelf) moveUnitIndicator.SetActive(false);

            if (curTowerMesh.sharedMesh == null) return;
            curTowerMesh.sharedMesh = null;
        }

        private void TowerButton(string towerName)
        {
            var tempTower = towerDictionary[towerName].towerLevels[0];
            towerTypeName = towerName;

            curTowerMesh.transform.SetPositionAndRotation(_buildTransform.position, _buildTransform.rotation);
            curTowerMesh.sharedMesh = tempTower.towerMesh.sharedMesh;

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
            _curSelectedTower = StackObjectPool.Get<Tower>(towerTypeName, _buildTransform);
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

            if (tempTower.TowerType == Tower.Type.Barracks)
            {
                tempTower.GetComponent<BarracksUnitTower>().UnitHealth = tt.health;
            }

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
            _tooltip.Show(_cam.WorldToScreenPoint(_toolTipTarget.position), info, towerName);
            _okButtonTarget = _eventSystem.currentSelectedGameObject.transform;
            _okButton.GetComponent<Button>().interactable = CanBuildCheck();
            _okButton.SetActive(true);
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