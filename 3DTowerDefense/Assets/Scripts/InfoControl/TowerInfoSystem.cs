using GameControl;
using ManagerControl;
using UnityEngine;

namespace InfoControl
{
    public class TowerInfoSystem : Singleton<TowerInfoSystem>
    {
        [SerializeField] private InputManager input;

        [SerializeField] private TowerInfo towerInfo;
        [SerializeField] private GameObject editPanel;

        private void Awake()
        {
            input.OnCancelPanelEvent += Hide;
            input.OnCancelPanelEvent += CloseEditPanel;
        }

        public void OpenInfo(Vector3 pos, string content, string header = "")
        {
            towerInfo.SetText(content, header);
            towerInfo.transform.position = pos;
            towerInfo.gameObject.SetActive(true);

            CloseEditPanel();
            input.isEdit = true;
            editPanel.transform.position = pos + Vector3.down * 20;
            editPanel.SetActive(true);
        }

        private void Hide()
        {
            towerInfo.gameObject.SetActive(false);
        }

        private void CloseEditPanel()
        {
            if (!editPanel.activeSelf) return;
            input.isEdit = false;
            editPanel.SetActive(false);
        }
    }
}