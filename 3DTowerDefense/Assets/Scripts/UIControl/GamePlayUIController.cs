using System;
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
        private Camera _cam;
        private EventSystem _eventSystem;
        private Tween _towerSelectPanelTween;

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
        private bool _isOnUI;

        private bool _isMoveUnit;

        private Dictionary<string, TowerData> _towerDictionary;

        [SerializeField] private UIManager uiManager;

        [SerializeField] private Button startButton;

        [SerializeField] private ToolTipSystem tooltip;

        [SerializeField] private GameObject startPanel;

        [SerializeField] private GameObject towerSelectPanel;

        // [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject towerButtons;
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
        [SerializeField] private LayerMask clickLayer;

        /*======================================================================================================================
         *                                        Unity Event
         ======================================================================================================================*/
        private void Awake()
        {
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
            towerSelectPanel.SetActive(true);
            okButton.SetActive(false);
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _isOnUI = _eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                }

                if (!_isOnUI && touch.phase == TouchPhase.Ended)
                {
                    OpenPanelPlease();
                }
            }
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
        }

        /*======================================================================================================================
         *                                   Init
         ======================================================================================================================*/
        private void Init()
        {
            Time.timeScale = 0;

            var towerButtonObj = new Button[towerButtons.transform.childCount];
            for (var i = 0; i < towerButtonObj.Length; i++)
            {
                towerButtonObj[i] = towerButtons.transform.GetChild(i).gameObject.GetComponent<Button>();
                var t = towerButtonObj[i].GetComponent<TowerButton>();
                towerButtonObj[i].onClick.AddListener(() => TowerSelectButtons(t.TowerTypeName));
            }

            _towerSelectPanelTween = towerSelectPanel.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
            _towerSelectPanelTween.PlayBackwards();
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

            MoveUIPlease(towerSelectPanel.transform);
        }

        private void MoveUIPlease(Transform ui)
        {
            var targetPos = _cam.WorldToScreenPoint(_uiTarget.position);
            ui.position = Vector3.Lerp(ui.position, targetPos, 1);
        }

        private void OpenPanelPlease()
        {
            var ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);

            Physics.Raycast(ray, out var hit);
            var buildPoint = hit.collider.CompareTag("BuildingPoint");
            var thisIsTower = hit.collider.CompareTag("Tower");
            if (buildPoint || thisIsTower)
            {
                curTowerMeshRenderer.enabled = false;
                towerRangeIndicator.enabled = false;
                tooltip.Hide();
                _panelIsOpen = true;
                _uiTarget = hit.transform;
                _toolTipTarget = towerSelectPanel.transform;
                _buildTransform = hit.transform;

                _isTower = thisIsTower;
                if (hit.collider.TryGetComponent(out Tower t))
                {
                    _curSelectedTower = t;
                    var indicatorTransform = towerRangeIndicator.transform;
                    indicatorTransform.position = _curSelectedTower.transform.position;
                    indicatorTransform.localScale = new Vector3(t.TowerRange * 2, 0.1f, t.TowerRange * 2);
                }

                towerButtons.SetActive(buildPoint);
                upgradeButton.SetActive(thisIsTower && t.TowerLevel != 2);
                aUpgradeButton.SetActive(thisIsTower && !t.IsUniqueTower && t.TowerLevel == 2);
                bUpgradeButton.SetActive(thisIsTower && !t.IsUniqueTower && t.TowerLevel == 2);
                moveUnitButton.SetActive(thisIsTower && t.TowerType == Tower.Type.Barracks);
                sellButton.SetActive(thisIsTower);
                okButton.SetActive(false);
                _towerSelectPanelTween.Restart();
            }
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

            tooltip.Hide();
            Close();
        }

        private void Close()
        {
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