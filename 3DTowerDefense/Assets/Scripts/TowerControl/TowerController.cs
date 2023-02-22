using System;
using System.Text;
using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TowerControl
{
    public class TowerController : MonoBehaviour
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private int _towerIndex;

        private int _numOfBuildingPoint;

        [SerializeField] private InputManager input;
//============================================================================================================
//====================================Tower Select Panel==========================================

        [SerializeField] private BuildingPointController buildingPointController;
        [FormerlySerializedAs("buildPanel")] [SerializeField] private GameObject towerSelectPanel;
        [SerializeField] private GameObject buildOkButton;

        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private string[] towersName;

        [SerializeField] private TowerManager[] towerManagers;
//====================================Tower Select Panel==========================================
//============================================================================================================

//============================================================================================================
//====================================Tower Edit Panel==========================================        

        private Tower _selectedTower;
        private Tower _tempTower;
        private bool _isUnique;

        [FormerlySerializedAs("editPanel")] [SerializeField] private GameObject towerEditPanel;
        [FormerlySerializedAs("infoPanel")] [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton, bUpgradeButton;
        [SerializeField] private GameObject upgradeOkButton;
        [SerializeField] private GameObject sellButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

//====================================Tower Edit Panel==============================================
//============================================================================================================

//============================================================================================================
//===================================Click Check================================================
        private Vector2 _cursorPos;
        private Camera _cam;

        [SerializeField] private LayerMask buildingPointLayerMask;

        private void Awake()
        {
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
            _cam = Camera.main;
            input.OnCancelPanelEvent += CloseTowerSelectPanel;
            input.OnCancelPanelEvent += CloseTowerEditPanel;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnClickEvent += ClickCheck;

            input.isBuild = false;
            input.isEdit = false;
            towerSelectPanel.SetActive(false);
            buildOkButton.SetActive(false);
            towerEditPanel.SetActive(false);
            towerInfoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
        }
//============================================================================================================
//====================================Tower Select Panel==========================================

        private void OpenTowerSelectPanel(int index, Vector3 pos, Quaternion rot, Vector3 cursorPos)
        {
            if (towerSelectPanel.activeSelf) return;
            _numOfBuildingPoint = index;
            _buildPosition = pos;
            _buildRotation = rot;
            input.isBuild = true;
            towerSelectPanel.transform.position = cursorPos;
            towerSelectPanel.SetActive(true);
        }

        private void CloseTowerSelectPanel()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            buildOkButton.SetActive(false);
            if (!_tempTower) return;
            _tempTower.gameObject.SetActive(false);
            _tempTower = null;
        }

        public void TowerSelectButton(int index)
        {
            if (_tempTower) _tempTower.gameObject.SetActive(false);
            print(towersName[index]);
            _tempTower = StackObjectPool.Get<Tower>(towersName[index], _buildPosition, _buildRotation);
            print(_tempTower);
            buildOkButton.transform.position = towerButtons[index].transform.position;
            buildOkButton.SetActive(true);

            TowerInfoSetText(towerManagers[index].towerLevels[0].towerInfo,
                towerManagers[index].towerLevels[0].towerName);
            towerInfoPanel.SetActive(true);
            _towerIndex = index;
        }

        public void TowerBuildButton()
        {
            input.isBuild = false;
            towerSelectPanel.SetActive(false);
            buildOkButton.SetActive(false);
            _selectedTower = _tempTower;
            _tempTower = null;
            _selectedTower.gameObject.SetActive(false);
            _selectedTower = StackObjectPool.Get<Tower>(towersName[_towerIndex], _buildPosition, _buildRotation);
            _selectedTower.Init();
            _selectedTower.towerNum = _towerIndex;
            _selectedTower.OnGetTowerInfoEvent += GetTowerInfo;
            _isUnique = _selectedTower.towerLevel >= 2;
            TowerUpgrade().Forget();
            buildingPointController.DeActiveBuildingPoint(_numOfBuildingPoint);
            towerInfoPanel.SetActive(false);
        }

        private void GetTowerInfo(Tower t)
        {
            _selectedTower = t;
            if (_selectedTower.towerLevel >= towerManagers.Length - 1)
            {
                sellButton.SetActive(true);
                aUpgradeButton.SetActive(false);
                bUpgradeButton.SetActive(false);
                upgradeButton.SetActive(false);
                return;
            }

            _isUnique = t.towerLevel >= 2;
            OpenTowerEditPanel(_cam.WorldToScreenPoint(t.transform.position));

            TowerInfoSetText(towerManagers[t.towerNum].towerLevels[t.towerLevel + 1].towerInfo,
                towerManagers[t.towerNum].towerLevels[t.towerLevel + 1].towerName);
        }

//====================================Tower Select Panel==========================================
//============================================================================================================

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

//============================================================================================================
//====================================Tower Edit Panel==============================================

        private void OpenTowerEditPanel(Vector3 pos)
        {
            if (towerEditPanel.activeSelf) return;
            input.isEdit = true;
            towerEditPanel.transform.position = pos;
            towerEditPanel.SetActive(true);
            CheckLevel();
        }

        private void CheckLevel()
        {
            aUpgradeButton.SetActive(_isUnique);
            bUpgradeButton.SetActive(_isUnique);
            upgradeButton.SetActive(!_isUnique);
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

        private bool _typeA;

        public void UniqueUpgradeButton(int index)
        {
            upgradeOkButton.transform.position =
                index == 1 ? aUpgradeButton.transform.position : bUpgradeButton.transform.position;
            upgradeOkButton.SetActive(true);
            if (index == 1)
            {
                TowerInfoSetText(
                    towerManagers[_selectedTower.towerNum].towerLevels[_selectedTower.towerLevel + 1].towerInfo,
                    towerManagers[_selectedTower.towerNum].towerLevels[_selectedTower.towerLevel + 1].towerName);
                _typeA = true;
            }
            else
            {
                TowerInfoSetText(
                    towerManagers[_selectedTower.towerNum].towerLevels[_selectedTower.towerLevel + 2].towerInfo,
                    towerManagers[_selectedTower.towerNum].towerLevels[_selectedTower.towerLevel + 2].towerName);
                _typeA = false;
            }

            towerInfoPanel.SetActive(true);
        }

        public void UpgradeOkButton()
        {
            if (_isUpgrade) return;
            TowerUpgrade().Forget();

            CloseTowerEditPanel();
        }

        private bool _isUpgrade;

        private async UniTask TowerUpgrade()
        {
            _isUpgrade = true;
            var t = _selectedTower;
            // _selectedTower = null;
            t.towerLevel++;
            if (_isUnique)
            {
                t.towerLevel = _typeA ? 3 : 4;
            }

            var towerIndexLevel = towerManagers[t.towerNum].towerLevels[t.towerLevel];

            t.meshFilter.sharedMesh = towerIndexLevel.consMesh.sharedMesh;

            // 1초 대기
            await UniTask.Delay(TimeSpan.FromSeconds(1));

            t.meshFilter.sharedMesh = towerIndexLevel.towerMesh.sharedMesh;

            t.atkRange = towerIndexLevel.attackRange;
            t.cooldown.cooldownTime = towerIndexLevel.attackCoolDown;
            _isUpgrade = false;
        }

//====================================Tower Edit Panel==============================================
//============================================================================================================

        private void CursorPosition(Vector2 pos)
        {
            _cursorPos = pos;
        }

        private void ClickCheck()
        {
            var r = _cam.ScreenPointToRay(_cursorPos);

            // =========If BuildingPoint=========================================================
            if (Physics.Raycast(r, out var hit, 1000, buildingPointLayerMask))
            {
                var position = hit.transform.position;
                OpenTowerSelectPanel(hit.transform.GetComponent<BuildingPoint>().index, position,
                    hit.transform.rotation, _cam.WorldToScreenPoint(position));
            }
        }
        
        [SerializeField] private Transform buildingPoint;
        [SerializeField] private BuildingPoint[] buildingPoints;
    }
}