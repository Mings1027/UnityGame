using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private Tower _selectedTower;
        private Tower _tempTower;
        private int _towerIndex;
        private Transform _buildingPoint;
        private EventSystem _eventSystem;
        private bool _isPause;
        private bool _isTowerSelected;
        private bool _isSell;
        private int _uniqueLevel;

        public static bool pointer;

        [SerializeField] private TowerBuildPointManager towerBuildPointManager;
        [SerializeField] private InputManager input;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        [SerializeField] private string[] towerNames;

        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private FollowWorld towerPanels;
        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject okButton;
        [SerializeField] private GameObject menuPanel;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject uniqueUpgradeButtons;

        [SerializeField] private GameObject sellButton;

        [SerializeField] private GameObject moveUnitButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        [SerializeField] private GameObject towerRangeIndicator;

        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            towerNames = new string[towerSelectPanel.transform.childCount];
            for (var i = 0; i < towerNames.Length; i++)
            {
                towerNames[i] = towerSelectPanel.transform.GetChild(i).GetComponent<Button>().name
                    .Replace("Button", "");
            }

            for (var i = 0; i < towerSelectPanel.transform.childCount; i++)
            {
                var index = i;
                towerSelectPanel.transform.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() => TowerSelectButton(index));
            }

            upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButton);
            for (var i = 0; i < uniqueUpgradeButtons.transform.childCount; i++)
            {
                var index = i;
                uniqueUpgradeButtons.transform.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() => UniqueUpgradeButton(index + 3));
            }

            sellButton.GetComponent<Button>().onClick.AddListener(SellButton);
            okButton.GetComponent<Button>().onClick.AddListener(OkButton);

            moveUnitButton.GetComponent<Button>().onClick.AddListener(() => input.isMoveUnit = true);
        }

        private void Start()
        {
            input.onPauseEvent += Pause;
            input.onResumeEvent += Resume;
            input.onClosePanelEvent += CloseTowerSelectPanel;
            input.onClosePanelEvent += CloseTowerEditPanel;
            input.onClosePanelEvent += () => okButton.SetActive(false);
            input.onClosePanelEvent += () => towerInfoPanel.SetActive(false);
            input.onClosePanelEvent += () => input.isPanelOpened = false;

            towerInfoPanel.SetActive(false);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.SetActive(false);
            towerPanels.target = transform;
            menuPanel.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointer = true;
            // print("On");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointer = false;
            // print("Off");
        }

        private void Pause()
        {
            _isPause = true;
            menuPanel.SetActive(true);
            Time.timeScale = 0;
        }

        private void Resume()
        {
            _isPause = false;
            menuPanel.SetActive(false);
            Time.timeScale = 1;
        }

        public void OpenTowerSelectPanel(Transform buildPoint)
        {
            input.isPanelOpened = true;
            CloseTowerSelectPanel();
            CloseTowerEditPanel();
            _buildingPoint = buildPoint;
            towerPanels.target = _buildingPoint;
            towerSelectPanel.SetActive(true);
        }

        private void CloseTowerSelectPanel()
        {
            towerSelectPanel.SetActive(false);
            okButton.SetActive(false);
            if (!_tempTower) return;
            _tempTower.gameObject.SetActive(false);
            _tempTower = null;
        }

        private void OpenTowerEditPanel(Tower t, Transform pos)
        {
            if (_selectedTower == t) return;
            input.isPanelOpened = true;
            CloseTowerSelectPanel();
            _isTowerSelected = true;

            if (_selectedTower != null)
            {
                _selectedTower.IsSelected = false;
            }

            _selectedTower = t;
            _selectedTower.IsSelected = true;

            //현재 선택한 타워가 BarracksTower면 BarracksUnit을 움직이게 하는 버튼을 켠다.
            moveUnitButton.SetActive(_selectedTower.Type == Tower.TowerType.Barracks);

            towerRangeIndicator.transform.position = _selectedTower.transform.position;
            towerRangeIndicator.transform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);
            towerRangeIndicator.SetActive(true);

            switch (t.TowerLevel)
            {
                case >= 3:
                    upgradeButton.SetActive(false);
                    uniqueUpgradeButtons.SetActive(false);
                    break;
                case 2:
                    upgradeButton.SetActive(false);
                    uniqueUpgradeButtons.SetActive(true);
                    break;
                default:
                    upgradeButton.SetActive(true);
                    uniqueUpgradeButtons.SetActive(false);
                    break;
            }

            towerPanels.target = pos;
            towerEditPanel.SetActive(true);
        }

        private void CloseTowerEditPanel()
        {
            if (!_isTowerSelected) return;
            _isTowerSelected = false;
            _selectedTower.IsSelected = false;
            _selectedTower = null;
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            towerRangeIndicator.SetActive(false);
        }

        public void TowerSelectButton(int index)
        {
            if (_tempTower) _tempTower.gameObject.SetActive(false);
            _tempTower = towerBuildPointManager.BuildTower(towerNames[index]);

            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];

            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        public void UpgradeButton()
        {
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[_selectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        //index should be 3 or 4 Cuz It's Unique Tower Upgrade 
        public void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        public void OkButton()
        {
            if (_isTowerSelected)
            {
                if (_isSell)
                {
                    SellTower();
                }
                else
                {
                    TowerUpgrade(_uniqueLevel, _selectedTower).Forget();
                }
            }
            else
            {
                TowerBuild();
            }

            CloseTowerEditPanel();
            okButton.SetActive(false);
        }

        public void SellButton()
        {
            _isSell = true;
            var coin = towerLevelManagers[(int)_selectedTower.Type].towerLevels[_selectedTower.TowerLevel].coin;
            ActiveOkButton("타워처분", $"이 타워를 처분하면 {coin} 골드가 반환됩니다.");
        }

        private void SellTower()
        {
            _isSell = false;
            _selectedTower.gameObject.SetActive(false);
            var towerTransform = _selectedTower.transform;
            var position = towerTransform.position;

            StackObjectPool.Get("BuildSmoke", position);
            StackObjectPool.Get("BuildingPoint", position, towerTransform.rotation);
        }

        private void TowerBuild()
        {
            towerSelectPanel.SetActive(false);
            _selectedTower = _tempTower;
            _tempTower = null;
            _selectedTower.gameObject.SetActive(false);
            _selectedTower = towerBuildPointManager.BuildTower(towerNames[_towerIndex]);

            _selectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
            _selectedTower.onResetMeshEvent += ResetMesh;

            _buildingPoint.gameObject.SetActive(false);
            towerInfoPanel.SetActive(false);
            TowerUpgrade(0, _selectedTower).Forget();
            _selectedTower = null;
        }

        private void ResetMesh(MeshFilter meshFilter)
        {
            meshFilter.sharedMesh = towerLevelManagers[_towerIndex].towerLevels[0].towerMesh.sharedMesh;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            okButton.GetComponent<FollowObject>().target = _eventSystem.currentSelectedGameObject.transform;
            TowerInfoSetText(info, towerName);
            towerInfoPanel.SetActive(true);
            okButton.SetActive(true);
        }

        private void TowerInfoSetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }

            contentField.text = content;
        }

        private async UniTaskVoid TowerUpgrade(int level, Tower selectedTower)
        {
            selectedTower.TowerLevelUp(level);
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position);
            var t = towerLevelManagers[(int)selectedTower.Type];
            var tt = t.towerLevels[selectedTower.TowerLevel];
            selectedTower.ReadyToBuild(tt.consMesh);

            await UniTask.Delay(1000);

            selectedTower.Building(t.hasUnit, tt.towerMesh, tt.attackDelay, tt.attackRange, tt.Damage, tt.health);
        }
    }
}