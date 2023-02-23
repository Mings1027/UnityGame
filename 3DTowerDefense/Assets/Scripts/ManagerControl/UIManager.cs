using Cysharp.Threading.Tasks;
using GameControl;
using TMPro;
using TowerControl;
using TowerControl.TowerControlFolder;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private Tower _selectedTower;
        private Tower _tempTower;
        private int _towerIndex;
        private GameObject _buildingPoint;

        public static bool Pointer { get; private set; }

        [SerializeField] private TowerManager towerManager;
        [SerializeField] private TowerLeveling towerLeveling;

        [SerializeField] private InputManager input;
        [SerializeField] private TowerLevelManager[] towerManagers;

        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private GameObject towerBuildButton;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton, bUpgradeButton;
        [SerializeField] private GameObject upgradeOkButton;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private string[] towersName;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            _cam = Camera.main;

            input.OnCancelPanelEvent += CloseTowerSelectPanel;
            input.OnCancelPanelEvent += CloseTowerEditPanel;

            towerButtons = new GameObject[towerSelectPanel.transform.childCount];
            towersName = new string[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = towerSelectPanel.transform.GetChild(i).gameObject;
                towersName[i] = towerButtons[i].name.Replace("Button", "");
            }
        }

        private void Start()
        {
            input.isBuild = false;
            input.isEdit = false;
            towerSelectPanel.SetActive(false);
            towerBuildButton.SetActive(false);
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
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

        public void OpenTowerSelectPanel(GameObject bp)
        {
            if (towerSelectPanel.activeSelf) return;
            input.isBuild = true;
            _buildingPoint = bp;
            towerSelectPanel.transform.position = _cam.WorldToScreenPoint(bp.transform.position);
            towerSelectPanel.SetActive(true);
        }

        private void CloseTowerSelectPanel()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            towerBuildButton.SetActive(false);
            if (!_tempTower) return;
            _tempTower.gameObject.SetActive(false);
            _tempTower = null;
        }

        public void TowerSelectButton(int index)
        {
            if (_tempTower) _tempTower.gameObject.SetActive(false);
            _tempTower = towerManager.ShowTempTower(towersName[index]);
            towerBuildButton.transform.position = towerButtons[index].transform.position;
            towerBuildButton.SetActive(true);

            var tempTowerLevel = towerManagers[index].towerLevels[0];
            TowerInfoSetText(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        public void TowerBuildButton()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            towerBuildButton.SetActive(false);
            _selectedTower = _tempTower;
            _tempTower = null;
            _selectedTower.gameObject.SetActive(false);
            _selectedTower = towerManager.BuildTower(towersName[_towerIndex]);
            _selectedTower.OnGetTowerInfoEvent += GetTowerInfo;
            _selectedTower.OnOpenTowerEditPanelEvent += OpenTowerEditPanel;

            _buildingPoint.SetActive(false);
            towerInfoPanel.SetActive(false);
            towerLeveling.TowerUpgrade(_selectedTower, towerManagers).Forget();
        }

        private void GetTowerInfo(Tower t)
        {
            _selectedTower = t;
            if (t.towerLevel >= towerManagers.Length - 1)
            {
                sellButton.SetActive(true);
                aUpgradeButton.SetActive(false);
                bUpgradeButton.SetActive(false);
                upgradeButton.SetActive(false);
                return;
            }

            towerLeveling.isUnique = t.towerLevel >= 2;
            var towerState = towerManagers[(int)t.towerType].towerLevels[t.towerLevel + 1];
            TowerInfoSetText(towerState.towerInfo, towerState.towerName);
        }

        private void OpenTowerEditPanel(Vector3 pos)
        {
            if (towerEditPanel.activeSelf) return;
            input.isEdit = true;
            towerEditPanel.transform.position = _cam.WorldToScreenPoint(pos);
            towerEditPanel.SetActive(true);

            aUpgradeButton.SetActive(towerLeveling.isUnique);
            bUpgradeButton.SetActive(towerLeveling.isUnique);
            upgradeButton.SetActive(!towerLeveling.isUnique);
        }

        private void CloseTowerEditPanel()
        {
            input.isEdit = false;
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
        }

        public void UpgradeButton()
        {
            upgradeOkButton.transform.position = upgradeButton.transform.position;
            upgradeOkButton.SetActive(true);
            towerInfoPanel.SetActive(true);
        }

        public void UniqueUpgradeButton(int index)
        {
            upgradeOkButton.transform.position =
                index == 1 ? aUpgradeButton.transform.position : bUpgradeButton.transform.position;
            upgradeOkButton.SetActive(true);
            var towerLv = index == 1 ? _selectedTower.towerLevel + 1 : _selectedTower.towerLevel + 2;
            towerLeveling.typeA = index == 1;
            var towerState = towerManagers[(int)_selectedTower.towerType].towerLevels[towerLv];
            TowerInfoSetText(towerState.towerInfo, towerState.towerName);

            towerInfoPanel.SetActive(true);
        }

        public void UpgradeOkButton()
        {
            if (towerLeveling.isUpgrade) return;
            towerLeveling.TowerUpgrade(_selectedTower, towerManagers).Forget();

            CloseTowerEditPanel();
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