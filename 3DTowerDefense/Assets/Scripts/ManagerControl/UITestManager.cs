using System;
using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManagerControl
{
    public class UITestManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Camera _cam;
        private Tower _selectedTower;
        private Tower _tempTower;
        private int _towerIndex;
        private int _uniqueLevel;
        private Transform _buildingPoint;
        private EventSystem _eventSystem;
        private BarracksTower _barrackTower;
        private GameObject _towerPanelGameObject;

        private Vector3 _buildPos;
        private Quaternion _buildRot;

        private bool _isPause;
        private bool _isSell;
        private bool _isMoveUnit;
        private bool _isTower;

        public bool pointer;

        // [SerializeField] private TowerBuildPointManager towerBuildPointManager;
        [SerializeField] private InputManager input;

        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        [SerializeField] private string[] towerNames;

        [SerializeField] private FollowWorld towerPanels;

        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject uniqueUpgradeButtons;

        [SerializeField] private GameObject okButton;
        [SerializeField] private GameObject menuPanel;

        [SerializeField] private GameObject sellButton;

        [SerializeField] private GameObject moveUnitButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        [SerializeField] private GameObject towerRangeIndicator;

        [SerializeField] private float moveUnitRange;

        private void Start()
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

            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            input.onPauseEvent += Pause;
            input.onResumeEvent += Resume;
            input.onClickEvent += CheckWhatIsIt;

            towerInfoPanel.SetActive(false);
            towerSelectPanel.SetActive(false);
            towerEditPanel.SetActive(false);
            okButton.SetActive(false);
            towerPanels.target = transform;
            _towerPanelGameObject = towerPanels.gameObject;
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

        private void CheckWhatIsIt()
        {
            if (pointer) return;
            var ray = _cam.ScreenPointToRay(input.mousePos);
            if (Physics.Raycast(ray, out var hit))
            {
                _isTower = hit.collider.CompareTag("Tower");
                towerRangeIndicator.SetActive(hit.collider.CompareTag("Tower"));

                if (hit.collider.CompareTag("BuildingPoint"))
                {
                    _towerPanelGameObject.SetActive(true);
                    OpenTowerSelectPanel(hit.transform);
                    towerEditPanel.SetActive(false);
                }
                else if (hit.collider.CompareTag("Tower"))
                {
                    _towerPanelGameObject.SetActive(true);
                    OpenTowerEditPanel(hit.collider.GetComponent<Tower>(), hit.transform);
                    towerSelectPanel.SetActive(false);
                }
                else if (_barrackTower != null && hit.collider.CompareTag("Ground") &&
                         Vector3.Distance(_barrackTower.transform.position, hit.point) < moveUnitRange)
                {
                    _barrackTower.MoveUnit(hit.point);
                }
                else
                {
                    if (_towerPanelGameObject.activeSelf)
                    {
                        _towerPanelGameObject.SetActive(false);
                    }

                    _isTower = false;
                }
            }

            ClosePanels();
        }

        private void OpenTowerSelectPanel(Transform t)
        {
            _buildPos = t.position;
            _buildRot = t.rotation;
            _buildingPoint = t;
            towerPanels.target = _buildingPoint;
            towerSelectPanel.SetActive(true);
        }

        private void OpenTowerEditPanel(Tower t, Transform trans)
        {
            _selectedTower = t;

            moveUnitButton.SetActive(_selectedTower.Type == Tower.TowerType.Barracks);

            towerRangeIndicator.transform.position = _selectedTower.transform.position;
            towerRangeIndicator.transform.localScale =
                new Vector3(_selectedTower.TowerRange * 2, 0.1f, _selectedTower.TowerRange * 2);

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

            towerPanels.target = trans;
            towerEditPanel.SetActive(true);
        }

        private void ClosePanels()
        {
            if (okButton.activeSelf)
            {
                okButton.SetActive(false);
            }

            if (towerInfoPanel.activeSelf)
            {
                towerInfoPanel.SetActive(false);
            }

            if (!_tempTower) return;
            _tempTower.gameObject.SetActive(false);
            _tempTower = null;
        }

        private void TowerSelectButton(int index)
        {
            if (_tempTower) _tempTower.gameObject.SetActive(false);
            _tempTower = StackObjectPool.Get<Tower>(towerNames[index], _buildPos, _buildRot);

            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];

            ActiveOkButton(tempTowerLevel.towerInfo, tempTowerLevel.towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        private void UpgradeButton()
        {
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[_selectedTower.TowerLevel + 1];
            ActiveOkButton(towerLevel.towerInfo, towerLevel.towerName);
        }

        //index should be 3 or 4 Cuz It's Unique Tower Upgrade 
        private void UniqueUpgradeButton(int index)
        {
            _uniqueLevel = index;
            var towerLevel = towerLevelManagers[(int)_selectedTower.Type].towerLevels[index];
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

            ClosePanels();
            towerEditPanel.SetActive(false);
        }

        private void SellButton()
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
            ClosePanels();
            _selectedTower = StackObjectPool.Get<Tower>(towerNames[_towerIndex], _buildPos, _buildRot);
            _selectedTower.onResetMeshEvent += ResetMesh;
            _buildingPoint.gameObject.SetActive(false);
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

        private void MoveUnitButton()
        {
            _isMoveUnit = true;
            _barrackTower = _selectedTower.GetComponent<BarracksTower>();
            ClosePanels();
            //베럭 범위 인디케이터 표시
            //
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