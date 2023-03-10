using BuildControl;
using GameControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManagerControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private TowerBase _selectedTower;
        private TowerBase _tempTower;
        private int _towerIndex;
        private GameObject _buildingPoint;
        private EventSystem _eventSystem;
        private bool _isPause;
        private bool _isTower;
        private bool _isSell;
        private int _uniqueLevel;
        private bool _pointer;
        private GameObject _curOpenPanel;

        [SerializeField] private TowerBuildPointManager towerBuildPointManager;
        [SerializeField] private TowerLeveling towerLeveling;
        [SerializeField] private InputManager input;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        [SerializeField] private GameObject menuPanel;
        [SerializeField] private string[] towerNames;

        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject towerInfoPanel;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject uniqueUpgradeButtons;

        [SerializeField] private GameObject okButton;
        [SerializeField] private GameObject sellButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        [SerializeField] private GameObject towerRangeObject;

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
        }

        private void Start()
        {
            input.onPauseEvent += Pause;
            input.onResumeEvent += Resume;
            input.onClosePanelEvent += CloseTowerSelectPanel;
            input.onClosePanelEvent += CloseTowerEditPanel;
            menuPanel.SetActive(false);
            towerSelectPanel.SetActive(false);
            okButton.SetActive(false);
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            okButton.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointer = true;
            // print("On");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointer = false;
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

        public void OpenTowerSelectPanel(GameObject buildPoint)
        {
            CloseTowerEditPanel();
            _buildingPoint = buildPoint;
            towerSelectPanel.transform.position = _cam.WorldToScreenPoint(buildPoint.transform.position);
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

        private void OpenTowerEditPanel(TowerBase t, Vector3 pos)
        {
            CloseTowerSelectPanel();
            _isTower = true;
            _selectedTower = t;
            towerRangeObject.transform.position = _selectedTower.transform.position;
            towerRangeObject.transform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);
            towerRangeObject.SetActive(true);

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

            towerEditPanel.transform.position = _cam.WorldToScreenPoint(pos);
            towerEditPanel.SetActive(true);
        }

        private void CloseTowerEditPanel()
        {
            _isTower = false;
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            okButton.SetActive(false);
            towerRangeObject.SetActive(false);
        }

        public void TowerSelectButton(int index)
        {
            okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            okButton.SetActive(true);

            if (_tempTower) _tempTower.gameObject.SetActive(false);
            _tempTower = towerBuildPointManager.BuildTower(towerNames[index]);

            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            TowerInfoSetText(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
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
            if (_isTower)
            {
                if (_isSell)
                {
                    SellTower();
                }
                else
                {
                    TowerLeveling.TowerUpgrade(_uniqueLevel, _selectedTower,
                        towerLevelManagers[(int)_selectedTower.Type]).Forget();
                }
            }
            else
            {
                TowerBuild();
            }

            CloseTowerEditPanel();
        }

        public void SellButton()
        {
            _isSell = true;
            okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            okButton.SetActive(true);
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
            okButton.SetActive(false);
            _selectedTower = _tempTower;
            _tempTower = null;
            _selectedTower.gameObject.SetActive(false);
            _selectedTower = towerBuildPointManager.BuildTower(towerNames[_towerIndex]);

            _selectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
            _selectedTower.onResetMeshEvent += ResetMesh;

            _buildingPoint.SetActive(false);
            towerInfoPanel.SetActive(false);
            TowerLeveling.TowerUpgrade(0, _selectedTower, towerLevelManagers[(int)_selectedTower.Type]).Forget();
        }

        private void ResetMesh(MeshFilter meshFilter)
        {
            meshFilter.sharedMesh = towerLevelManagers[_towerIndex].towerLevels[0].towerMesh.sharedMesh;
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