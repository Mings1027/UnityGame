using Cysharp.Threading.Tasks;
using GameControl;
using TMPro;
using TowerControl;
using TowerControl.TowerControlFolder;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private GameObject _buildingPoint;
        private EventSystem _eventSystem;
        private bool _isUniqueUpgrade;
        private bool _isTower;

        public static bool Pointer { get; private set; }

        [SerializeField] private TowerManager towerManager;
        [SerializeField] private TowerLeveling towerLeveling;

        [SerializeField] private InputManager input;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject towerInfoPanel;

        [SerializeField] private Button[] towerButtons;
        [SerializeField] private string[] towerNames;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject uniqueUpgradeButtons;

        [SerializeField] private GameObject okButton;

        [SerializeField] private GameObject sellButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            input.OnCancelPanelEvent += CloseTowerSelectPanel;
            input.OnCancelPanelEvent += CloseTowerEditPanel;

            towerButtons = new Button[towerSelectPanel.transform.childCount];
            towerNames = new string[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = towerSelectPanel.transform.GetChild(i).GetComponent<Button>();
                towerNames[i] = towerButtons[i].name.Replace("Button", "");
            }
        }

        private void Start()
        {
            input.isBuild = false;
            input.isEdit = false;
            towerSelectPanel.SetActive(false);
            okButton.SetActive(false);
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            okButton.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Pointer = true;
            print("On");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Pointer = false;
            print("Off");
        }

        public void OpenTowerSelectPanel(GameObject buildPoint)
        {
            if (towerSelectPanel.activeSelf) return;
            input.isBuild = true;
            _buildingPoint = buildPoint;
            towerSelectPanel.transform.position = _cam.WorldToScreenPoint(buildPoint.transform.position);
            towerSelectPanel.SetActive(true);
        }

        public void TowerSelectButton(int index)
        {
            okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            okButton.SetActive(true);

            if (_tempTower) _tempTower.gameObject.SetActive(false);
            _tempTower = towerManager.SpawnTower(towerNames[index]);

            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            TowerInfoSetText(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        public void UpgradeButton()
        {
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[_selectedTower.towerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        public void UniqueUpgradeButton(int index)
        {
            towerLeveling.typeA = index == 1;
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[index == 1 ? 3 : 4];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        public void OkButton()
        {
            if (_isTower)
            {
                towerLeveling.TowerUpgrade(_selectedTower, towerLevelManagers[(int)_selectedTower.Type]);
            }
            else
            {
                TowerBuild();
            }

            CloseTowerEditPanel();
        }

        public void SellButton()
        {
            _selectedTower.gameObject.SetActive(false);
        }

        private void CloseTowerSelectPanel()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            okButton.SetActive(false);
            if (!_tempTower) return;
            _tempTower.gameObject.SetActive(false);
            _tempTower = null;
        }

        private void TowerBuild()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            okButton.SetActive(false);
            _selectedTower = _tempTower;
            _tempTower = null;
            _selectedTower.gameObject.SetActive(false);
            _selectedTower = towerManager.SpawnTower(towerNames[_towerIndex]);
            _selectedTower.OnGetTowerInfoEvent += GetTowerInfo;
            _selectedTower.OnOpenTowerEditPanelEvent += OpenTowerEditPanel;

            _buildingPoint.SetActive(false);
            towerInfoPanel.SetActive(false);
            towerLeveling.TowerUpgrade(_selectedTower, towerLevelManagers[(int)_selectedTower.Type]);
        }

        private void GetTowerInfo(Tower t)
        {
            _isTower = true;
            _selectedTower = t;
            switch (t.towerLevel)
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
        }

        private void OpenTowerEditPanel(Vector3 pos)
        {
            if (_selectedTower.isUpgrading) return;
            if (towerEditPanel.activeSelf) return;
            input.isEdit = true;
            towerEditPanel.transform.position = _cam.WorldToScreenPoint(pos);
            towerEditPanel.SetActive(true);
        }

        private void CloseTowerEditPanel()
        {
            _isTower = false;
            input.isEdit = false;
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            okButton.SetActive(false);
        }

        private void ActiveOkButton(string info, string towerName)
        {
            okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            okButton.SetActive(true);
            TowerInfoSetText(info, towerName);
            towerInfoPanel.SetActive(true);
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
    }
}