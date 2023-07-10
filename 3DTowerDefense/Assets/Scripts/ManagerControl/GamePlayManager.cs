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
        private Transform _tooltipTarget;

        private Image _aButtonImage;
        private Image _bButtonImage;

        private string _towerTypeName;

        [Serializable]
        private struct GamePlayInfo
        {
            public int _uniqueLevel;
            public int _sellTowerCoin;
            public int _lastSelectedTowerButtonIndex;
            public int _lastSelectedEditButtonIndex;

            public bool _panelIsOpen;
            public bool _isSell;
            public bool _isTower;
        }

        private GamePlayInfo gamePlayInfo;
        private Button[] _towerSelectButtons;

        private Dictionary<string, TowerData> _towerDictionary;
        private Dictionary<int, Action> _towerEditBtnDic;

        private Action _upgradeButtonEvent;
        private Action _aUpgradeButtonEvent;
        private Action _bUpgradeButtonEvent;
        private Action _moveUnitButtonEvent;
        private Action _sellButtonEvent;

        [SerializeField] private WaveManager waveManager;
        [SerializeField] private InfoUIController infoUIController;

        [SerializeField] private Button checkButton;

        [Header("----------Game Play----------")] [SerializeField, Space(10)]
        private GameObject gamePlayPanel;

        [SerializeField] private GameObject towerButtonsPanel;

        [SerializeField] private TowerButtonController towerButtons;
        [SerializeField] private TowerEditButtonController towerEditButtons;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton;
        [SerializeField] private GameObject bUpgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private ToolTipSystem tooltip;

        [Header("----------Game Over----------")] [SerializeField, Space(10)]
        private GameObject gameOverPanel;

        [SerializeField] private Button reStartButton;
        [SerializeField] private Button mainMenuButton;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer curTowerMeshRenderer;
        [SerializeField] private SpriteRenderer towerRangeIndicator;
        [SerializeField] private MoveUnitIndicator moveUnitIndicator;
        [SerializeField] private SpriteRenderer moveUnitSprite;
        [SerializeField] private float moveUnitRange;

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
            checkButton.onClick.AddListener(SetUIButton);
            TowerButtonsInit();
            TowerEditButtonInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            waveManager.ReStart();
            UIInit();
        }

        private void LateUpdate()
        {
            MoveUI();
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

        private void TowerButtonsInit()
        {
           gamePlayInfo. _lastSelectedTowerButtonIndex = -1;
            _towerSelectButtons = new Button[towerButtons.transform.childCount];
            for (var i = 0; i < _towerSelectButtons.Length; i++)
            {
                _towerSelectButtons[i] = towerButtons.transform.GetChild(i).GetComponent<Button>();
                var index = i;
                _towerSelectButtons[i].onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySound(SoundManager.ButtonSound);
                    var towerType = _towerSelectButtons[index].name.Replace("Button", "");
                    ClickTowerButtons(towerType, index);
                });
            }

            _towerSelectPanelTween = towerButtonsPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
        }

        private void TowerEditButtonInit()
        {
            gamePlayInfo._lastSelectedEditButtonIndex = -1;
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
                    SoundManager.Instance.PlaySound(SoundManager.ButtonSound);
                    ClickEditButtons(index);
                });
            }

            _aButtonImage = aUpgradeButton.transform.GetChild(0).GetComponent<Image>();
            _bButtonImage = bUpgradeButton.transform.GetChild(0).GetComponent<Image>();
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

        private void UIInit()
        {
            gamePlayPanel.SetActive(false);
            infoUIController.gameObject.SetActive(false);
        }

        public void ReStart()
        {
            Time.timeScale = 1;
            checkButton.enabled = true;
            gameOverPanel.SetActive(false);
            gamePlayPanel.SetActive(true);
            infoUIController.gameObject.SetActive(true);
        }

        public void GameOver()
        {
            Time.timeScale = 0;
            checkButton.enabled = false;
            gamePlayPanel.SetActive(false);
            gameOverPanel.SetActive(true);
        }

        public void MapGenerator(int stageIndex)
        {
            Destroy(_curMap);
            _curMap = Instantiate(mapPrefabs[stageIndex], transform);
            MapInit();
            BuildPointInit();
        }

        private void MapInit()
        {
            var wayPointsParent = _curMap.transform.Find("WayPointsParent");
            waveManager.WayPointList = new Vector3[wayPointsParent.childCount];

            for (var i = 0; i < waveManager.WayPointList.Length; i++)
            {
                waveManager.WayPointList[i] = wayPointsParent.GetChild(i).position;
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
            if (!gamePlayInfo._panelIsOpen) return;
            var targetPos = _cam.WorldToScreenPoint(_curUITarget.position);
            towerButtonsPanel.transform.position = targetPos;
        }

        private void SetUIButton()
        {
            if (moveUnitIndicator.gameObject.activeSelf) return;

            var touch = Input.GetTouch(0);
            CheckWhatIsIt(touch.position, touch.deltaPosition);
        }

        private void CheckWhatIsIt(Vector3 clickPos, Vector2 delta)
        {
            if (delta != Vector2.zero) return;

            Physics.Raycast(_cam.ScreenPointToRay(clickPos), out var hit);

            if (hit.collider.CompareTag("Tower"))
            {
                SoundManager.Instance.PlaySound(SoundManager.BuildPointSound);
                ResetUI();
                SetEditButtons(hit);
                OpenEditButtonPanel();
            }
            else if (hit.collider.CompareTag("BuildingPoint"))
            {
                SoundManager.Instance.PlaySound(SoundManager.BuildPointSound);
                ResetUI();
                OpenTowerButtonPanel(hit);
            }
            else
            {
                gamePlayInfo._panelIsOpen = false;
                gamePlayInfo._isTower = false;
                gamePlayInfo._isSell = false;
                _towerSelectPanelTween.PlayBackwards();
                OffIndicator();
            }
        }

        private void OpenTowerButtonPanel(RaycastHit hit)
        {
            _towerSelectPanelTween.Restart();
            if (!towerButtons.gameObject.activeSelf) towerButtons.gameObject.SetActive(true);
            if (_curUITarget == hit.transform) return;

            gamePlayInfo._panelIsOpen = true;

            var t = hit.transform;
            _curUITarget = t;
            _tooltipTarget = towerButtons.transform;
        }

        private void OpenEditButtonPanel()
        {
            _towerSelectPanelTween.Restart();
            if (!towerEditButtons.gameObject.activeSelf) towerEditButtons.gameObject.SetActive(true);
            gamePlayInfo._panelIsOpen = true;
            gamePlayInfo._isTower = true;

            if (_curSelectedTower.TowerType == Tower.Type.Barracks) return;
            var targetingTower = _curSelectedTower.GetComponent<TargetingTower>();
            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(targetingTower.TowerStat.towerRange * 2, targetingTower.TowerStat.towerRange * 2, 0);

            towerRangeIndicator.enabled = true;
        }

        private void SetEditButtons(RaycastHit hit)
        {
            var tower = hit.transform.GetComponent<Tower>();
            if (tower.TowerLevel == 2) SetUpgradeButtonImage(tower);
            _curUITarget = hit.transform;
            _curSelectedTower = tower;
            _tooltipTarget = towerEditButtons.transform;

            upgradeButton.SetActive(tower.TowerLevel != 2);
            aUpgradeButton.SetActive(!tower.IsUniqueTower && tower.TowerLevel == 2);
            bUpgradeButton.SetActive(!tower.IsUniqueTower && tower.TowerLevel == 2);
            moveUnitButton.SetActive(tower.TowerType == Tower.Type.Barracks);
            sellButton.SetActive(true);
        }

        private void ClickTowerButtons(string towerName, int index)
        {
            if (index == gamePlayInfo._lastSelectedTowerButtonIndex)
            {
                if (!infoUIController.CheckBuildCoin(0)) return;
                OkButton();
                return;
            }

            gamePlayInfo._lastSelectedTowerButtonIndex = index;
            ShowSelectTower(towerName);
        }

        private void ShowSelectTower(string towerName)
        {
            var tempTower = _towerDictionary[towerName].towerLevels[0];
            _towerTypeName = towerName;

            curTowerMesh.transform.SetPositionAndRotation(_curUITarget.position, _curUITarget.rotation);
            curTowerMesh.sharedMesh = tempTower.towerMesh.sharedMesh;
            curTowerMeshRenderer.enabled = true;
            tooltip.Show(_tooltipTarget, tempTower.towerInfo, tempTower.towerName);

            if (_towerDictionary[towerName].unitTower)
            {
                ShowUnitTowerInfo();
            }
            else
            {
                ShowTargetingTowerInfo(tempTower);
            }
        }

        private void ShowUnitTowerInfo()
        {
            towerRangeIndicator.enabled = false;

            var moveIndicatorTransform = moveUnitSprite.transform;
            moveIndicatorTransform.localScale = new Vector3(moveUnitRange, moveUnitRange);
            moveIndicatorTransform.position = curTowerMesh.transform.position;
            moveUnitSprite.enabled = true;
        }

        private void ShowTargetingTowerInfo(TowerData.TowerLevelData tempTower)
        {
            moveUnitSprite.enabled = false;

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = curTowerMesh.transform.position;
            var range = tempTower.attackRange * 2;
            indicatorTransform.localScale = new Vector3(range, range);

            if (towerRangeIndicator.enabled) return;
            towerRangeIndicator.enabled = true;
        }

        private void ClickEditButtons(int index)
        {
            if (index == gamePlayInfo._lastSelectedEditButtonIndex)
            {
                if (infoUIController.CheckBuildCoin(_curSelectedTower.TowerLevel + 1) || gamePlayInfo._isSell)
                {
                    OkButton();
                    return;
                }
            }

            gamePlayInfo._lastSelectedEditButtonIndex = index;
            _towerEditBtnDic[index].Invoke();
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

        private void ResetUI()
        {
            gamePlayInfo._isTower = false;
            gamePlayInfo._isSell = false;
            gamePlayInfo._lastSelectedTowerButtonIndex = -1;
            gamePlayInfo._lastSelectedEditButtonIndex = -1;
            towerButtons.DefaultSprite();
            towerEditButtons.DefaultSprite();
            tooltip.Hide();

            OffButtons();
            OffIndicator();
        }

        private void OffButtons()
        {
            if (towerEditButtons.gameObject.activeSelf) towerEditButtons.gameObject.SetActive(false);
            if (sellButton.activeSelf) sellButton.SetActive(false);
            if (towerButtons.gameObject.activeSelf) towerButtons.gameObject.SetActive(false);
        }

        private void OffIndicator()
        {
            if (curTowerMeshRenderer.enabled) curTowerMeshRenderer.enabled = false;
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (!moveUnitSprite.enabled) return;

            moveUnitIndicator.gameObject.SetActive(false);
        }

        private void TowerBuild()
        {
            _curSelectedTower = ObjectPoolManager.Get<Tower>(_towerTypeName, _curUITarget);
            _curUITarget.gameObject.SetActive(false);
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
                tempTower.TowerUniqueLevelUp(gamePlayInfo._uniqueLevel);
                tt = t.towerUniqueLevels[tempTower.TowerUniqueLevel];
                infoUIController.DecreaseCoin(3);
            }

            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, tempTower.transform.position);

            tempTower.BuildTowerWithDelay(tt.consMesh, tt.minDamage, tt.maxDamage, tt.attackRange, tt.attackDelay, tt.health);

            await UniTask.Delay(1000);

            tempTower.BuildTower(tt.towerMesh);
        }

        private void UpgradeButton()
        {
            gamePlayInfo.    _isSell = false;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType]
                .towerLevels[_curSelectedTower.TowerLevel + 1];
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
        }

        private void AUniqueUpgradeButton()
        {
            gamePlayInfo._isSell = false;
            gamePlayInfo._uniqueLevel = 0;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType].towerUniqueLevels[0];
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
        }

        private void BUniqueUpgradeButton()
        {
            gamePlayInfo._isSell = false;
            gamePlayInfo._uniqueLevel = 1;
            var towerLevel = towerData[(int)_curSelectedTower.TowerType].towerUniqueLevels[1];
            tooltip.Show(_tooltipTarget, towerLevel.towerInfo, towerLevel.towerName);
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            OffButtons();
            OffIndicator();

            moveUnitIndicator.BarracksTower = _curSelectedTower.GetComponent<BarracksUnitTower>();
            var moveIndicatorTransform = moveUnitIndicator.transform;
            moveIndicatorTransform.position = _curSelectedTower.transform.position;
            moveIndicatorTransform.localScale = new Vector3(moveUnitRange, moveUnitRange);
            moveUnitIndicator.gameObject.SetActive(true);
        }

        private void SellButton()
        {
            gamePlayInfo._isSell = true;
            gamePlayInfo._sellTowerCoin = infoUIController.GetTowerCoin(_curSelectedTower);
            tooltip.Show(_tooltipTarget, $"이 타워를 처분하면{gamePlayInfo._sellTowerCoin.ToString()} 골드가 반환됩니다.", "타워처분");
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.IsUniqueTower ? SoundManager.SellSound3 :
                _curSelectedTower.TowerLevel > 0 ? SoundManager.SellSound2 : SoundManager.SellSound1);

            _curSelectedTower.gameObject.SetActive(false);
            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);
            ObjectPoolManager.Get(PoolObjectName.BuildingPoint, _curSelectedTower.transform);

            infoUIController.IncreaseCoin(gamePlayInfo._sellTowerCoin);
        }

        private void OkButton()
        {
            if (gamePlayInfo._isSell) SellTower();
            else
            {
                if (!gamePlayInfo._isTower) TowerBuild();
                TowerUpgrade().Forget();
            }

            ResetUI();
        }
    }
}