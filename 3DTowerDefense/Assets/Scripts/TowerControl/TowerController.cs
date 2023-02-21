using System;
using BuildControl;
using GameControl;
using ManagerControl;
using TMPro;
using UnityEngine;

namespace TowerControl
{
    public class TowerController : MonoBehaviour
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private int _towerIndex;
        private int _numOfBuildingPoint;
        private Tower _tempSelectedTower;

        [SerializeField] private InputManager input;
//============================================================================================================
//====================================Build Panel==========================================

        [SerializeField] private BuildingPointController buildingPointController;
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject buildOkButton;

        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private string[] towersName;

        [SerializeField] private TowerManager[] towerManagers;
//====================================Build Panel==========================================
//============================================================================================================

//============================================================================================================
//====================================Edit Panel==========================================        

        private Tower _selectedTower;
        private bool _isUnique;

        [SerializeField] private GameObject editPanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject aUpgradeButton, bUpgradeButton;
        [SerializeField] private GameObject upgradeOkButton;

        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

//====================================Edit Panel==============================================
//============================================================================================================

//============================================================================================================
//===================================Click Check================================================
        private Vector2 _cursorPos;
        private Camera _cam;

        [SerializeField] private LayerMask towerLayer;
        [SerializeField] private LayerMask buildingPointLayerMask;

        private void Awake()
        {
            towerButtons = new GameObject[buildPanel.transform.childCount];
            towersName = new string[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildPanel.transform.GetChild(i).gameObject;
                towersName[i] = towerButtons[i].name.Replace("Button", "");
            }
        }

        private void Start()
        {
            _cam = Camera.main;
            input.OnCancelPanelEvent += CloseBuildPanel;
            input.OnCancelPanelEvent += CloseEditPanel;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnClickEvent += ClickCheck;

            input.isBuild = false;
            input.isEdit = false;
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
            editPanel.SetActive(false);
            infoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
        }
//============================================================================================================
//====================================Build Panel==========================================

        private void OpenBuildPanel(int index, Vector3 pos, Quaternion rot, Vector3 cursorPos)
        {
            if (buildPanel.activeSelf) return;
            _numOfBuildingPoint = index;
            _buildPosition = pos;
            _buildRotation = rot;
            input.isBuild = true;
            buildPanel.transform.position = cursorPos;
            buildPanel.SetActive(true);
        }

        private void CloseBuildPanel()
        {
            input.isBuild = false;
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
            if (!_tempSelectedTower) return;
            _tempSelectedTower.gameObject.SetActive(false);
        }

        public void TowerSelectButton(int index)
        {
            if (_tempSelectedTower) _tempSelectedTower.gameObject.SetActive(false);
            _tempSelectedTower = StackObjectPool.Get<Tower>(towersName[index], _buildPosition, _buildRotation);
            buildOkButton.transform.position = towerButtons[index].transform.position;
            buildOkButton.SetActive(true);

            TowerInfoSetText(towerManagers[index].towerLevels[index].towerInfo,
                towerManagers[index].towerLevels[index].towerName);

            _towerIndex = index;
        }

        public void TowerBuildButton()
        {
            StackObjectPool.Get<Tower>(towersName[_towerIndex], _buildPosition, _buildRotation).Init();
            buildingPointController.DeActiveBuildingPoint(_numOfBuildingPoint);
            CloseBuildPanel();
        }


//====================================Build Panel==========================================
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
//====================================Edit Panel==============================================

        private void OpenEditPanel(Vector3 pos, string content, string header = "")
        {
            if (editPanel.activeSelf) return;
            CheckLevel();
            input.isEdit = true;
            editPanel.transform.position = pos;
            editPanel.SetActive(true);
            TowerInfoSetText(content, header);
        }

        private void CheckLevel()
        {
            aUpgradeButton.SetActive(_isUnique);
            bUpgradeButton.SetActive(_isUnique);
            upgradeButton.SetActive(!_isUnique);
        }

        private void CloseEditPanel()
        {
            input.isEdit = false;
            editPanel.SetActive(false);
            infoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
        }

        public void UpgradeButton()
        {
            upgradeOkButton.transform.position = upgradeButton.transform.position;
            upgradeOkButton.SetActive(true);
            infoPanel.SetActive(true);
        }

        public void UniqueUpgradeButton(int index)
        {
            upgradeOkButton.transform.position =
                index == 1 ? aUpgradeButton.transform.position : bUpgradeButton.transform.position;
            upgradeOkButton.SetActive(true);
            infoPanel.SetActive(true);
        }

        public void UpgradeOkButton()
        {
            if (_isUnique) _selectedTower.UniqueUpgrade();
            else _selectedTower.Upgrade();

            CloseEditPanel();
        }

//====================================Edit Panel==============================================
//============================================================================================================

        private void CursorPosition(Vector2 pos)
        {
            _cursorPos = pos;
        }

        private void ClickCheck()
        {
            var r = _cam.ScreenPointToRay(_cursorPos);
// ======================================If BuildingPoint=========================================================

            if (Physics.Raycast(r, out var hit, 1000, buildingPointLayerMask))
            {
                var position = hit.transform.position;
                OpenBuildPanel(hit.transform.GetComponent<BuildingPoint>().index, position,
                    hit.transform.rotation, _cam.WorldToScreenPoint(position));
            }

//============================================If Tower===========================================================

            if (Physics.Raycast(r, out hit, 1000, towerLayer))
            {
                _selectedTower = hit.transform.GetComponent<Tower>();
                var s = _selectedTower;
                _isUnique = s.towerLevel >= 2;
                OpenEditPanel(_cam.WorldToScreenPoint(hit.transform.position),
                    s.towerManager.towerLevels[s.towerLevel + 1].towerInfo,
                    s.towerManager.towerLevels[s.towerLevel + 1].towerName);
                print(s.towerLevel);
            }
        }
    }
}