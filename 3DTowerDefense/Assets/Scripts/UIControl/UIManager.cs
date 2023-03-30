using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : Singleton<UIManager>, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private Tower _selectedTower;
        private int _towerIndex;
        private int _uniqueLevel;
        private Transform _buildingPoint;
        private EventSystem _eventSystem;

        private Vector3 _buildPos;
        private Quaternion _buildRot;

        private bool _isPause, _isSell, _isTower, _panelIsOpen;

        public static bool pointer;

        [SerializeField] private InputManager input;
        [SerializeField] private Transform towerBuildPoints;
        [SerializeField] private TowerLevelManager[] towerLevelManagers;
        [SerializeField] private string[] towerNames;
        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private Mesh[] towerMesh;
        [SerializeField] private FollowWorld towerPanels;
        [SerializeField] private GameObject towerSelectPanel;

        [Header("Tower Edit")] [Space(10)] [SerializeField]
        private GameObject towerEditPanel;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject uniqueUpgradeButtons;
        [SerializeField] private GameObject sellButton;
        [SerializeField] private GameObject moveUnitButton;

        [SerializeField] private FollowWorld okButton;

        [Header("Menu")] [Space(10)] [SerializeField]
        private GameObject menuPanel;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private GameObject towerInfoPanel;

        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        [SerializeField] private GameObject towerRangeIndicator;
        [SerializeField] private GameObject barrackRangeIndicator;

        private void Start()
        {
            _cam = Camera.main;
            _eventSystem = EventSystem.current;

            for (var i = 0; i < towerBuildPoints.transform.childCount; i++)
            {
                var child = towerBuildPoints.transform.GetChild(i);
                StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position, child.rotation)
                    .onOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }

            towerNames = new string[towerSelectPanel.transform.childCount];
            for (var i = 0; i < towerNames.Length; i++)
            {
                towerNames[i] = towerSelectPanel.transform.GetChild(i).GetComponent<Button>().name
                    .Replace("Button", "");
            }

            towerMesh = new Mesh[4];
            for (var i = 0; i < towerMesh.Length; i++)
            {
                towerMesh[i] = towerLevelManagers[i].towerLevels[0].towerMesh.sharedMesh;
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

            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            input.onPauseEvent += Pause;
            input.onResumeEvent += Resume;
            input.onMoveUnitEvent += MoveUnit;
            input.onClosePanelEvent += CloseUI;

            towerInfoPanel.SetActive(false);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.gameObject.SetActive(false);
            towerPanels.WorldTarget(transform);
            menuPanel.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointer = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointer = false;
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

        private void MoveUnit()
        {
            var ray = _cam.ScreenPointToRay(input.mousePos);
            if (Physics.Raycast(ray, out var hit))
            {
                if (Vector3.Distance(_selectedTower.transform.position, hit.point) < _selectedTower.TowerRange)
                {
                    if (!hit.collider.CompareTag("Ground")) return;
                    input.isMoveUnit = false;
                    barrackRangeIndicator.SetActive(false);
                    _selectedTower.GetComponent<BarracksTower>().MoveUnits(hit.point);
                }
                else
                {
                    print("Can't Move");
                }
            }
        }

        private void OpenTowerSelectPanel(Transform t)
        {
            ResetUI();
            _panelIsOpen = true;
            _isTower = false;
            _buildPos = t.position;
            _buildRot = t.rotation;
            _buildingPoint = t;
            towerPanels.WorldTarget(_buildingPoint);
            _selectedTower = t.GetComponent<Tower>();
            towerPanels.ActivePanel();
            towerSelectPanel.SetActive(true);
            towerEditPanel.SetActive(false);
        }

        private void OpenTowerEditPanel(Tower t, Transform trans)
        {
            ResetUI();
            _selectedTower = t;
            _panelIsOpen = true;
            _isTower = true;

            moveUnitButton.SetActive(_selectedTower.TowerType == Tower.Type.Barracks);

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

            towerPanels.WorldTarget(trans);
            towerEditPanel.SetActive(true);
            towerSelectPanel.SetActive(false);
            towerPanels.ActivePanel();
        }

        private void CloseUI()
        {
            if (!_panelIsOpen) return;

            _panelIsOpen = false;
            ResetUI();
        }

        private void ResetUI()
        {
            if (towerInfoPanel.activeSelf)
            {
                towerInfoPanel.SetActive(false);
            }

            if (okButton.gameObject.activeSelf)
            {
                okButton.DeActivePanel();
            }

            if (towerRangeIndicator.activeSelf)
            {
                towerRangeIndicator.SetActive(false);
            }

            if (barrackRangeIndicator.activeSelf)
            {
                barrackRangeIndicator.SetActive(false);
            }

            towerPanels.DeActivePanel();
            if (curTowerMesh.sharedMesh == null) return;
            curTowerMesh.sharedMesh = null;
        }

        private void TowerSelectButton(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            curTowerMesh.transform.SetPositionAndRotation(_buildPos, _buildRot);
            curTowerMesh.sharedMesh = tempTowerLevel.towerMesh.sharedMesh;

            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        private void UpgradeButton()
        {
            _isSell = false;
            var towerLevel = towerLevelManagers[(int)_selectedTower.TowerType]
                .towerLevels[_selectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        //index should be 3 or 4 Cuz It's Unique Tower Upgrade 
        private void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_selectedTower.TowerType].towerLevels[index];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
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
                    TowerUpgrade(_uniqueLevel, _selectedTower).Forget();
                }
            }
            else
            {
                TowerBuild();
            }

            CloseUI();
        }

        private void SellButton()
        {
            _isSell = true;
            var coin = towerLevelManagers[(int)_selectedTower.TowerType].towerLevels[_selectedTower.TowerLevel].coin;
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
            _selectedTower = StackObjectPool.Get<Tower>(towerNames[_towerIndex], _buildPos, _buildRot);
            _buildingPoint.gameObject.SetActive(false);
            TowerUpgrade(0, _selectedTower).Forget();

            _selectedTower.onResetMeshEvent += ResetMesh;
            _selectedTower.onOpenTowerEditPanelEvent += OpenTowerEditPanel;
        }

        private void ResetMesh(MeshFilter meshFilter)
        {
            meshFilter.sharedMesh = towerLevelManagers[_towerIndex].towerLevels[0].towerMesh.sharedMesh;
        }

        private void ActiveOkButton(string info, string towerName)
        {
            okButton.GetComponent<FollowWorld>().ScreenTarget(_eventSystem.currentSelectedGameObject.transform);
            TowerInfoSetText(info, towerName);
            towerInfoPanel.SetActive(true);
            okButton.ActivePanel();
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            input.isMoveUnit = true;
            ResetUI();
            barrackRangeIndicator.transform.position = _selectedTower.transform.position;
            barrackRangeIndicator.transform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);
            barrackRangeIndicator.SetActive(true);
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
            var t = towerLevelManagers[(int)selectedTower.TowerType];
            var tt = t.towerLevels[selectedTower.TowerLevel];
            selectedTower.ReadyToBuild(tt.consMesh);

            if (selectedTower.TowerType == Tower.Type.Barracks)
            {
                _selectedTower.GetComponent<BarracksTower>().UnitHealth = tt.health;
            }

            await UniTask.Delay(1000);

            selectedTower.Building(tt.towerMesh, tt.minDamage, tt.maxDamage, tt.attackRange, tt.attackDelay);

            _selectedTower = null;
        }
    }
}