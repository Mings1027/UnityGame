using System.Collections.Generic;
using BuildControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using MapControl;
using ToolTipControl;
using TowerControl;
using UIControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class GamePlayManager : MonoBehaviour
    {
        private Camera _cam;
        private EventSystem _eventSystem;
        private Tween _towerSelectPanelTween;

        private Tower _curSelectedTower;

        private GameObject _curMap;
        private Transform _curUITarget;
        private Transform _buildTransform;
        private Transform _okButtonTarget;
        private Transform _tooltipTarget;

        private string _towerTypeName;

        private int _selectedStageIndex;
        private int _uniqueLevel;
        private int _sellTowerCoin;

        private bool _panelIsOpen;
        private bool _enableOkBtn;
        private bool _isSell;
        private bool _isTower;
        private bool _isMoveUnit;

        private Dictionary<string, TowerData> _towerDictionary;

        [SerializeField] private WaveManager waveManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private MainMenuUIController mainMenuUIController;
        [SerializeField] private GameOverUIController gameOverUIController;

        [SerializeField] private GameObject gamePlayPanel;
        [SerializeField] private GameObject informationPanel;

        [SerializeField] private ToolTipSystem tooltip;
        [SerializeField] private GameObject towerButtons;
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

        [SerializeField] private GameObject[] mapPrefabs;

        /*======================================================================================================================
         *                                        Unity Event
         ======================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

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
        }

        private void LateUpdate()
        {
            MoveUI();
            if (_enableOkBtn)
            {
                okButton.transform.position = _okButtonTarget.position;
            }
        }

        private void OnDestroy()
        {
            _towerSelectPanelTween.Kill();
        }

        /*======================================================================================================================
         *                                   Init
         ======================================================================================================================*/
        private void Init()
        {
            _selectedStageIndex = -1;

            var towerButtonObj = new Button[towerButtons.transform.childCount];
            for (var i = 0; i < towerButtonObj.Length; i++)
            {
                towerButtonObj[i] = towerButtons.transform.GetChild(i).gameObject.GetComponent<Button>();
                var t = towerButtonObj[i].GetComponent<TowerButton>();
                towerButtonObj[i].onClick.AddListener(() => TowerSelectButtons(t.TowerTypeName));
            }

            _towerSelectPanelTween = gamePlayPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
        }

        public void ReStart()
        {
            gameOverUIController.SetGameOverPanel(false);
            gamePlayPanel.SetActive(true);
            informationPanel.SetActive(true);
            uiManager.Init(_selectedStageIndex);
            Close();
        }

        public void GameOver()
        {
            gameOverUIController.SetGameOverPanel(true);
            gamePlayPanel.SetActive(false);
            informationPanel.SetActive(false);
        }

        public void MapGenerator(int stageIndex)
        {
            if (_selectedStageIndex == stageIndex) return;

            _selectedStageIndex = stageIndex;
            Destroy(_curMap);
            _curMap = Instantiate(mapPrefabs[stageIndex], transform);
            mainMenuUIController.SetStageSelectPanel(false);
            MapInit();
            BuildPointInit();
        }

        private void MapInit()
        {
            var wayPointsParent = _curMap.transform.Find("WayPointsParent");
            waveManager.WayPointList = new Transform[wayPointsParent.childCount];
            for (var i = 0; i < waveManager.WayPointList.Length; i++)
            {
                waveManager.WayPointList[i] = wayPointsParent.GetChild(i);
            }
        }

        public void BuildPointInit()
        {
            var mapController = _curMap.GetComponent<MapController>();
            mapController.onCloseUIEvent += CloseUI;
            var buildPoint = mapController.TowerBuildPoint;
            for (var i = 0; i < buildPoint.childCount; i++)
            {
                var child = buildPoint.GetChild(i);
                ObjectPoolManager.Get<BuildingPoint>(PoolObjectName.BuildingPoint, child)
                    .onOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
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

        private void MoveUI()
        {
            if (!_panelIsOpen) return;
            var targetPos = _cam.WorldToScreenPoint(_curUITarget.position);
            gamePlayPanel.transform.position = Vector3.Lerp(gamePlayPanel.transform.position, targetPos, 1);
        }

        private void OpenTowerSelectPanel(Transform t)
        {
            CloseUI();
            _panelIsOpen = true;
            _isTower = false;
            _curUITarget = t;
            _buildTransform = t;
            _tooltipTarget = towerButtons.transform;

            towerButtons.SetActive(true);
            _towerSelectPanelTween.Restart();
        }

        private void OpenTowerEditPanel(Tower t)
        {
            CloseUI();
            _panelIsOpen = true;
            _isTower = true;
            _curUITarget = t.transform;
            _curSelectedTower = t;
            _tooltipTarget = towerEditPanel.transform;

            towerEditPanel.SetActive(true);
            upgradeButton.SetActive(t.TowerLevel != 2);
            aUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
            bUpgradeButton.SetActive(!t.IsUniqueTower && t.TowerLevel == 2);
            moveUnitButton.SetActive(t.TowerType == Tower.Type.Barracks);
            sellButton.SetActive(true);
            _towerSelectPanelTween.Restart();

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale = new Vector3(t.TowerRange * 2, 0.1f, t.TowerRange * 2);
            towerRangeIndicator.enabled = true;
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
            _panelIsOpen = false;
            _isSell = false;

            _towerSelectPanelTween.PlayBackwards();

            Close();
        }

        private void Close()
        {
            tooltip.Hide();

            if (towerButtons.activeSelf) towerButtons.SetActive(false);
            if (towerEditPanel.activeSelf) towerEditPanel.SetActive(false);
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (moveUnitIndicator.activeSelf) moveUnitIndicator.SetActive(false);
            if (okButton.activeSelf) okButton.SetActive(false);
            if (sellButton.activeSelf) sellButton.SetActive(false);
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
            _curSelectedTower = ObjectPoolManager.Get<Tower>(_towerTypeName, _buildTransform);
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

            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, tempTower.transform.position);
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
            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);
            ObjectPoolManager.Get(PoolObjectName.BuildingPoint, _curSelectedTower.transform);

            uiManager.TowerCoin += _sellTowerCoin;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            _enableOkBtn = true;
            tooltip.Show(_tooltipTarget, info, towerName);
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
            _enableOkBtn = false;
        }
    }
}