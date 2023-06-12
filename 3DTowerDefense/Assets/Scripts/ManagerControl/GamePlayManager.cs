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

        private Image _aButtonImage;
        private Image _bButtonImage;

        private string _towerTypeName;

        private int _uniqueLevel;
        private int _sellTowerCoin;
        private int _lastSelectedTowerButtonIndex;
        private int _lastSelectedEditButtonIndex;

        private bool _panelIsOpen;
        private bool _enableOkBtn;
        private bool _isSell;
        private bool _isTower;
        private bool _isMoveUnit;

        private Dictionary<string, TowerData> _towerDictionary;

        [SerializeField] private WaveManager waveManager;
        [SerializeField] private MainMenuUIController mainMenuUIController;
        [SerializeField] private InfoUIController infoUIController;

        [Header("---Game Play---")] [Space(10)] [SerializeField]
        private GameObject gamePlayPanel;

        [SerializeField] private TowerButtonController towerButtons;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton;
        [SerializeField] private GameObject bUpgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private GameObject okButton;
        [SerializeField] private ToolTipSystem tooltip;

        [Header("---Game Over---")] [Space(10)] [SerializeField]
        private GameObject gameOverPanel;

        [SerializeField] private Button reStartButton;
        [SerializeField] private Button mainMenuButton;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer curTowerMeshRenderer;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private MoveUnitIndicator moveUnitIndicator;

        [SerializeField] private TowerData[] towerData;

        [SerializeField] private GameObject[] mapPrefabs;

        [SerializeField] private Sprite[] archerUpgradeImages;
        [SerializeField] private Sprite[] barracksUpgradeImages;
        [SerializeField] private Sprite[] canonUpgradeImages;
        [SerializeField] private Sprite[] mageUpgradeImages;

        /*======================================================================================================================
         *                                        Unity Event
         ======================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;
            TowerDataInit();
            TowerButtonInit();
            TowerEditButtonInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            waveManager.ReStart();
            mainMenuUIController.Init();
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

        private void TowerButtonInit()
        {
            _lastSelectedTowerButtonIndex = -1;
            var towerButtonObj = new Button[towerButtons.transform.childCount];
            for (var i = 0; i < towerButtonObj.Length; i++)
            {
                towerButtonObj[i] = towerButtons.transform.GetChild(i).GetChild(0).GetComponent<Button>();
                var t = towerButtonObj[i].GetComponent<TowerButton>();
                var index = i;
                towerButtonObj[i].onClick.AddListener(() => { TowerSelectButtons(t.TowerTypeName, index); });
            }

            _towerSelectPanelTween = gamePlayPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
        }

        private void TowerEditButtonInit()
        {
            var editButtons = new[]
            {
                upgradeButton.GetComponent<Button>(),
                aUpgradeButton.GetComponent<Button>(),
                bUpgradeButton.GetComponent<Button>(),
                moveUnitButton.GetComponent<Button>(),
                sellButton.GetComponent<Button>(),
                okButton.GetComponent<Button>()
            };
            foreach (var t in editButtons)
            {
                t.onClick.AddListener(() => { SoundManager.Instance.PlaySound("ButtonSound"); });
            }

            upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(0));
            bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(1));
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            _aButtonImage = aUpgradeButton.transform.GetChild(0).GetComponent<Image>();
            _bButtonImage = bUpgradeButton.transform.GetChild(0).GetComponent<Image>();

            moveUnitIndicator.onMoveUnitEvent += () => _isMoveUnit = false;
        }

        private void GameOverPanelInit()
        {
            reStartButton.onClick.AddListener(() =>
            {
                ObjectPoolManager.ReStart();
                ReStart();
                BuildPointInit();
                waveManager.ReStart();
            });
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        public void ReStart()
        {
            Time.timeScale = 1;
            gameOverPanel.SetActive(false);
            gamePlayPanel.SetActive(true);
            infoUIController.Init();
            Close();
        }

        public void GameOver()
        {
            Time.timeScale = 0;
            gameOverPanel.SetActive(true);
            gamePlayPanel.SetActive(false);
        }

        public void MapGenerator(int stageIndex)
        {
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

        private void BuildPointInit()
        {
            var mapController = _curMap.GetComponent<MapController>();
            var buildPoint = mapController.TowerBuildPoint;
            for (var i = 0; i < buildPoint.childCount; i++)
            {
                var child = buildPoint.GetChild(i);
                ObjectPoolManager.Get<BuildingPoint>(PoolObjectName.BuildingPoint, child);
            }
        }

        private void MoveUI()
        {
            if (!_panelIsOpen) return;
            var targetPos = _cam.WorldToScreenPoint(_curUITarget.position);
            gamePlayPanel.transform.position = targetPos;
        }

        public void SetUIButton()
        {
            if (_isMoveUnit) return;
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            Physics.Raycast(_cam.ScreenPointToRay(touch.position), out var hit);
            if (hit.collider.CompareTag("Tower"))
            {
                SoundManager.Instance.PlaySound("BuildPointTowerSound");
                OpenTowerEditPanel(hit);
            }
            else if (hit.collider.CompareTag("BuildingPoint"))
            {
                SoundManager.Instance.PlaySound("BuildPointTowerSound");
                OpenTowerSelectPanel(hit);
            }
            else
            {
                if (touch.deltaPosition != Vector2.zero) return;
                CloseUI();
            }
        }

        private void OpenTowerSelectPanel(RaycastHit hit)
        {
            CloseUI();
            _panelIsOpen = true;
            var t = hit.transform;
            _curUITarget = t;
            _buildTransform = t;
            _tooltipTarget = towerButtons.transform;

            towerButtons.gameObject.SetActive(true);
            _towerSelectPanelTween.Restart();
        }

        private void OpenTowerEditPanel(RaycastHit hit)
        {
            CloseUI();
            var tower = hit.transform.GetComponent<Tower>();
            SetUpgradeButtonImage(tower);
            _panelIsOpen = true;
            _isTower = true;
            _curUITarget = hit.transform;
            _curSelectedTower = tower;
            _tooltipTarget = towerEditPanel.transform;

            towerEditPanel.SetActive(true);
            upgradeButton.SetActive(tower.TowerLevel != 2);
            aUpgradeButton.SetActive(!tower.IsUniqueTower && tower.TowerLevel == 2);
            bUpgradeButton.SetActive(!tower.IsUniqueTower && tower.TowerLevel == 2);
            moveUnitButton.SetActive(tower.TowerType == Tower.Type.Barracks);
            sellButton.SetActive(true);
            _towerSelectPanelTween.Restart();

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale = tower.TowerType == Tower.Type.Barracks
                ? new Vector3(30, 0.1f, 30)
                : new Vector3(tower.TowerRange * 2, 0.1f, tower.TowerRange * 2);

            towerRangeIndicator.enabled = true;
        }

        private void SetUpgradeButtonImage(Tower t)
        {
            var towerType = t.TowerType;
            switch (towerType)
            {
                case Tower.Type.Archer:
                    _aButtonImage.sprite = archerUpgradeImages[0];
                    _bButtonImage.sprite = archerUpgradeImages[1];
                    break;
                case Tower.Type.Barracks:
                    _aButtonImage.sprite = barracksUpgradeImages[0];
                    _bButtonImage.sprite = barracksUpgradeImages[1];
                    break;
                case Tower.Type.Canon:
                    _aButtonImage.sprite = canonUpgradeImages[0];
                    _bButtonImage.sprite = canonUpgradeImages[1];
                    break;
                case Tower.Type.Mage:
                default:
                    _aButtonImage.sprite = mageUpgradeImages[0];
                    _bButtonImage.sprite = mageUpgradeImages[1];
                    break;
            }
        }

        private void MoveUnitButton()
        {
            _isMoveUnit = true;
            moveUnitButton.SetActive(false);
            CloseUI();
            moveUnitIndicator.BarracksTower = _curSelectedTower.GetComponent<BarracksUnitTower>();
            var moveUnitIndicatorTransform = moveUnitIndicator.transform;
            moveUnitIndicatorTransform.position = _curSelectedTower.transform.position;
            moveUnitIndicatorTransform.localScale = new Vector3(30, 0.1f, 30);
            moveUnitIndicator.gameObject.SetActive(true);
        }

        private void CloseUI()
        {
            _lastSelectedTowerButtonIndex = -1;
            towerButtons.DefaultSprite();
            if (!_panelIsOpen) return;
            _panelIsOpen = false;
            _isTower = false;
            _isSell = false;

            _towerSelectPanelTween.PlayBackwards();

            Close();
        }

        private void Close()
        {
            tooltip.Hide();

            if (towerButtons.gameObject.activeSelf) towerButtons.gameObject.SetActive(false);
            if (towerEditPanel.activeSelf) towerEditPanel.SetActive(false);
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (moveUnitIndicator.gameObject.activeSelf) moveUnitIndicator.gameObject.SetActive(false);
            if (okButton.activeSelf) okButton.SetActive(false);
            if (sellButton.activeSelf) sellButton.SetActive(false);
            curTowerMeshRenderer.enabled = false;
        }

        private void TowerSelectButtons(string towerName, int index)
        {
            if (index == _lastSelectedTowerButtonIndex)
            {
                OkButton();
                return;
            }

            _lastSelectedTowerButtonIndex = index;
            var tempTower = _towerDictionary[towerName].towerLevels[0];
            _towerTypeName = towerName;

            curTowerMesh.transform.SetPositionAndRotation(_buildTransform.position, _buildTransform.rotation);
            curTowerMesh.sharedMesh = tempTower.towerMesh.sharedMesh;
            curTowerMeshRenderer.enabled = true;
            tooltip.Show(_tooltipTarget, tempTower.towerInfo, tempTower.towerName);

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
                infoUIController.DecreaseCoin(tempTower.TowerLevel);
            }
            else
            {
                tempTower.TowerUniqueLevelUp(_uniqueLevel);
                tt = t.towerUniqueLevels[tempTower.TowerUniqueLevel];
                infoUIController.DecreaseCoin(3);
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
            _sellTowerCoin = infoUIController.GetTowerCoin(_curSelectedTower);
            ActiveOkButton($"이 타워를 처분하면{_sellTowerCoin.ToString()} 골드가 반환됩니다.", "타워처분");
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.IsUniqueTower ? "SellTower3" :
                _curSelectedTower.TowerLevel > 0 ? "SellTower2" : "SellTower1");

            _curSelectedTower.gameObject.SetActive(false);
            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);
            ObjectPoolManager.Get(PoolObjectName.BuildingPoint, _curSelectedTower.transform);

            infoUIController.IncreaseCoin(_sellTowerCoin);
        }

        private void ActiveOkButton(string info, string towerName)
        {
            _enableOkBtn = true;
            tooltip.Show(_tooltipTarget, info, towerName);
            _okButtonTarget = _eventSystem.currentSelectedGameObject.transform;
            okButton.SetActive(true);
            okButton.GetComponent<Button>().interactable
                = _isSell || infoUIController.CheckBuildCoin(_isTower ? _curSelectedTower.TowerLevel + 1 : 0);
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