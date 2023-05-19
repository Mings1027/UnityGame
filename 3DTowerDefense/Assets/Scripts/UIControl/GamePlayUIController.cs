using Cysharp.Threading.Tasks;
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
using UnityEngine.UI;

namespace UIControl
{
    public class GamePlayUIController : MonoBehaviour
    {
        private Camera _cam;
        private EventSystem _eventSystem;
        private InformationUIController _infoUIController;

        private int _towerIndex;
        private GameObject[] _towerButtons;
        private string[] towerNames;
        private Sequence _towerSelectPanelSequence;
        private Tween _towerEditPanelTween;
        private int _lastIndex;
        private int _uniqueLevel;
        private int _sellTowerCoin;

        private Tower _curSelectedTower;
        private Transform _buildTransform;
        private Transform _uiTarget;

        private bool _panelIsOpen;
        private bool _isSell;
        private bool _isTower;
        private bool _isTowerPanel, isEditPanel;
        private bool _isMoveUnit;

        [SerializeField] private GameObject startPanel;
        [SerializeField] private Button startButton;

        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private ToolTipSystem _tooltip;
        [SerializeField] private GameObject _upgradeButton;
        [SerializeField] private GameObject _aUpgradeButton;
        [SerializeField] private GameObject _bUpgradeButton;
        [SerializeField] private GameObject _moveUnitButton;
        [SerializeField] private GameObject _sellButton;
        [SerializeField] private GameObject _okButton;

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private GameObject moveUnitIndicator;
        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int towerCoin;

        private int TowerCoin
        {
            get
            {
                coinText.text = towerCoin.ToString();
                return towerCoin;
            }
            set
            {
                towerCoin = value;
                coinText.text = towerCoin.ToString();
            }
        }

        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;
            _infoUIController = InformationUIController.Instance;

            startButton.onClick.AddListener(StartGame);

            _upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            _aUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(3));
            _bUpgradeButton.GetComponent<Button>().onClick.AddListener(() => UniqueUpgradeButton(4));
            _moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            _sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            _okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            moveUnitIndicator.GetComponent<MoveUnitIndicator>().onMoveUnitEvent += MoveUnit;
        }

        private void Start()
        {
            Init();
            startPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            _okButton.SetActive(false);
        }

        private void LateUpdate()
        {
            MoveUI();
        }

        private void OnDestroy()
        {
            _towerSelectPanelSequence?.Kill();
        }

        private void Init()
        {
            coinText.text = towerCoin.ToString();
            _towerButtons = new GameObject[towerSelectPanel.transform.childCount];
            _towerSelectPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            for (var i = 0; i < _towerButtons.Length; i++)
            {
                _towerButtons[i] = towerSelectPanel.transform.GetChild(i).gameObject;
                var index = i;
                _towerButtons[i].GetComponent<Button>().onClick.AddListener(() => TowerButton(index));
                _towerSelectPanelSequence.Append(_towerButtons[i].transform.DOScale(1, 0.05f).From(0))
                    .Join(_towerButtons[i].transform.DOLocalMove(Vector3.zero, 0.05f).From());
            }

            towerNames = new string[_towerButtons.Length];
            for (int i = 0; i < towerNames.Length; i++)
            {
                towerNames[i] = _towerButtons[i].name.Replace("Button", "");
            }

            _towerEditPanelTween = towerEditPanel.transform.DOScale(1, 0.05f).From(0).SetAutoKill(false);

            WaveManager.Instance.onCoinIncreaseEvent += IncreaseCoin;
            GameManager.Instance.Map.GetComponent<MapController>().onCloseUIEvent += CloseUI;
        }

        private void StartGame()
        {
            startPanel.transform.DOMoveY(Screen.height, 0.5f).SetEase(Ease.InBack)
                .OnComplete(() => startPanel.SetActive(false));
        }

        private void IncreaseCoin(int amount)
        {
            TowerCoin += amount;
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
            if (!towerSelectPanel.activeSelf) towerSelectPanel.SetActive(true);
            _buildTransform = t;
            _towerSelectPanelSequence.Restart();
        }

        private void OpenTowerEditPanel(Tower t, Transform uiTarget)
        {
            CloseUI();
            if (!towerEditPanel.activeSelf)
                towerEditPanel.SetActive(true);
            _towerEditPanelTween.Restart();
            _isTower = true;
            _panelIsOpen = true;
            isEditPanel = true;
            _curSelectedTower = t;
            _uiTarget = uiTarget;

            _moveUnitButton.SetActive(_curSelectedTower.TowerType == Tower.Type.Barracks);

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position;
            indicatorTransform.localScale =
                new Vector3(_curSelectedTower.TowerRange * 2, 0.1f, _curSelectedTower.TowerRange * 2);

            if (!towerRangeIndicator.enabled)
                towerRangeIndicator.enabled = true;

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

            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;

            if (moveUnitIndicator.activeSelf) moveUnitIndicator.SetActive(false);

            if (curTowerMesh.sharedMesh == null) return;
            curTowerMesh.sharedMesh = null;
        }

        private void TowerButton(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            curTowerMesh.transform.SetPositionAndRotation(_buildTransform.position, _buildTransform.rotation);
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

        private void TowerBuild()
        {
            _curSelectedTower = StackObjectPool.Get<Tower>(towerNames[_towerIndex], _buildTransform);
            _curSelectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
            _buildTransform.gameObject.SetActive(false);

            TowerUpgrade().Forget();
        }

        private async UniTaskVoid TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            tempTower.TowerLevelUp(_uniqueLevel);
            StackObjectPool.Get("BuildSmoke", tempTower.transform.position);
            var tlm = towerLevelManagers[(int)tempTower.TowerType];
            var tl = tlm.towerLevels[tempTower.TowerLevel];

            var c = _curSelectedTower.TowerLevel > 3 ? 3 : _curSelectedTower.TowerLevel;
            TowerCoin -= _infoUIController.TowerCoin[c];

            tempTower.TowerInit(tl.consMesh);

            if (tempTower.TowerType == Tower.Type.Barracks)
            {
                tempTower.GetComponent<BarracksUnitTower>().UnitHealth = tl.health;
            }

            await UniTask.Delay(1000);

            tempTower.TowerSetting(tl.towerMesh, tl.minDamage, tl.maxDamage, tl.attackRange,
                tl.attackDelay);
        }

        private void UpgradeButton()
        {
            _isSell = false;
            var towerLevel = towerLevelManagers[(int)_curSelectedTower.TowerType]
                .towerLevels[_curSelectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        private void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_curSelectedTower.TowerType].towerLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        private void SellButton()
        {
            _isSell = true;
            _sellTowerCoin =
                _infoUIController.GetTowerCoin[_curSelectedTower.TowerLevel > 3 ? 3 : _curSelectedTower.TowerLevel];
            var getCoin = _sellTowerCoin.ToString();

            ActiveOkButton(string.Format("이 타워를 처분하면{0} 골드가 반환됩니다.", getCoin), "타워처분");
        }

        private void SellTower()
        {
            _isSell = false;
            _curSelectedTower.gameObject.SetActive(false);
            StackObjectPool.Get("BuildSmoke", _curSelectedTower.transform);
            StackObjectPool.Get("BuildingPoint", _curSelectedTower.transform);

            TowerCoin += _sellTowerCoin;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            _tooltip.Show(towerSelectPanel.transform.position, info, towerName);
            _okButton.transform.position = _isTower
                ? _eventSystem.currentSelectedGameObject.transform.position
                : _towerButtons[_towerIndex].transform.position;

            _okButton.GetComponent<Button>().interactable
                = _isSell ||
                  (_isTower && towerCoin >= _infoUIController.TowerCoin[_curSelectedTower.TowerLevel + 1]) ||
                  (!_isTower && towerCoin >= 70);

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
    }
}