using System;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class GamePlayManager : MonoBehaviour
    {
        private Camera _cam;
        private Tween _towerSelectPanelTween;

        private Tower _curSelectedTower;

        private GameObject _curMap;
        private Transform _curUITarget;
        private Transform _buildTransform;
        private Transform _tooltipTarget;

        private Image _aButtonImage;
        private Image _bButtonImage;

        private string _towerTypeName;

        private int _uniqueLevel;
        private int _sellTowerCoin;
        private int _lastSelectedTowerButtonIndex;
        private int _lastSelectedEditButtonIndex;

        private bool _panelIsOpen;
        private bool _isSell;
        private bool _isTower;
        private bool _isMoveUnit;

        private Dictionary<string, TowerData> _towerDictionary;
        private Dictionary<int, Action> _towerEditBtnDic;

        private Action _upgradeButtonEvent;
        private Action _aUpgradeButtonEvent;
        private Action _bUpgradeButtonEvent;
        private Action _moveUnitButtonEvent;
        private Action _sellButtonEvent;

        [SerializeField] private WaveManager waveManager;
        [SerializeField] private MainMenuUIController mainMenuUIController;
        [SerializeField] private InfoUIController infoUIController;

        [Header("---Game Play---")] [Space(10)] [SerializeField]
        private GameObject gamePlayPanel;

        [SerializeField] private TowerButtonController towerButtons;
        [SerializeField] private TowerEditButtonController towerEditButtons;
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
            // if (_enableOkBtn)
            // {
            //     okButton.transform.position = _okButtonTarget.position;
            // }
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
                towerButtonObj[i] = towerButtons.transform.GetChild(i).GetComponent<Button>();
                var t = towerButtonObj[i].GetComponent<TowerButton>();
                var index = i;
                towerButtonObj[i].onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySound("ButtonSound");
                    TowerSelectButtons(t.TowerTypeName, index);
                });
            }

            _towerSelectPanelTween = gamePlayPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
        }

        private void TowerEditButtonInit()
        {
            _lastSelectedEditButtonIndex = -1;
            _upgradeButtonEvent += UpgradeButton;
            _aUpgradeButtonEvent += AUniqueUpgradeButton;
            _bUpgradeButtonEvent += BUniqueUpgradeButton;
            _moveUnitButtonEvent += MoveUnitButton;
            _sellButtonEvent += SellButton;

            _towerEditBtnDic = new Dictionary<int, Action>
            {
                { 0, _upgradeButtonEvent },
                { 1, _aUpgradeButtonEvent },
                { 2, _bUpgradeButtonEvent },
                { 3, _moveUnitButtonEvent },
                { 4, _sellButtonEvent }
            };

            for (var i = 0; i < towerEditButtons.transform.childCount; i++)
            {
                var index = i;
                towerEditButtons.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySound("ButtonSound");
                    TowerEditButtons(index);
                });
            }

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

        private void TowerEditButtons(int index)
        {
            if (index == _lastSelectedEditButtonIndex)
            {
                OkButton();
                return;
            }

            _lastSelectedEditButtonIndex = index;
            _towerEditBtnDic[index].Invoke();
        }

        private void OpenTowerEditPanel(RaycastHit hit)
        {
            CloseUI();
            var tower = hit.transform.GetComponent<Tower>();
            if (tower.TowerLevel == 2) SetUpgradeButtonImage(tower);
            _panelIsOpen = true;
            _isTower = true;
            _curUITarget = hit.transform;
            _curSelectedTower = tower;
            _tooltipTarget = towerEditButtons.transform;

            towerEditButtons.gameObject.SetActive(true);
            var canUpgrade = infoUIController.CheckBuildCoin(_isTower ? _curSelectedTower.TowerLevel + 1 : 0);
            upgradeButton.SetActive(canUpgrade && tower.TowerLevel != 2);
            aUpgradeButton.SetActive(canUpgrade && !tower.IsUniqueTower && tower.TowerLevel == 2);
            bUpgradeButton.SetActive(canUpgrade && !tower.IsUniqueTower && tower.TowerLevel == 2);
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

            towerEditButtons.SetDefaultSprites(_aButtonImage.sprite, _bButtonImage.sprite);
        }

        private void CloseUI()
        {
            _lastSelectedTowerButtonIndex = -1;
            _lastSelectedEditButtonIndex = -1;
            towerButtons.DefaultSprite();
            towerEditButtons.DefaultSprite();
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
            if (towerEditButtons.gameObject.activeSelf) towerEditButtons.gameObject.SetActive(false);
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (moveUnitIndicator.gameObject.activeSelf) moveUnitIndicator.gameObject.SetActive(false);
            if (okButton.activeSelf) okButton.SetActive(false);
            if (sellButton.activeSelf) sellButton.SetActive(false);
            curTowerMeshRenderer.enabled = false;
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
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
        }

        private void AUniqueUpgradeButton()
        {
            _isSell = false;
            _uniqueLevel = 0;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType].towerUniqueLevels[0];
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
        }

        private void BUniqueUpgradeButton()
        {
            _isSell = false;
            _uniqueLevel = 1;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType].towerUniqueLevels[1];
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
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

        private void SellButton()
        {
            _isSell = true;
            _sellTowerCoin = infoUIController.GetTowerCoin(_curSelectedTower);
            tooltip.Show(_tooltipTarget, $"이 타워를 처분하면{_sellTowerCoin.ToString()} 골드가 반환됩니다.", "타워처분");
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