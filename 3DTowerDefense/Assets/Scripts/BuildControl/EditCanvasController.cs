using System;
using GameControl;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    public class EditCanvasController : BuildController
    {
        [SerializeField] private InputManager input;
      
        [SerializeField] private GameObject editPanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject upgradeOkButton;
        
        [Header("Tower Info")] [Space(10)] [SerializeField]
        private TextMeshProUGUI headerField;

        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            input.OnCancelPanelEvent += CloseEditPanel;
        }

        private void Start()
        {
            CloseEditPanel();
        }

        public void OpenEditPanel(Vector3 pos, string content, string header = "")
        {
            if (editPanel.activeSelf) return;
            input.isEdit = true;
            editPanel.transform.position = pos;
            editPanel.SetActive(true);
            TowerInfoSetText(content, header);
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
        
        private void CloseEditPanel()
        {
            input.isEdit = false;
            editPanel.SetActive(false);
            infoPanel.SetActive(false);
            upgradeOkButton.SetActive(false);
        }
        
        public void UpgradeButton()
        {
            upgradeOkButton.SetActive(true);
            infoPanel.SetActive(true);
        }

        public void UpgradeOkButton()
        {
            SelectedTower.Upgrade();
            CloseEditPanel();
        }

        public void UniqueUpgradeButton()
        {
            SelectedTower.UniqueUpgrade();
            CloseEditPanel();
        }

    }
}