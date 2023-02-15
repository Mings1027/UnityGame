using System;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace BuildControl
{
    public class EditController : Singleton<EditController>
    {
        [SerializeField] private InputManager input;

        [SerializeField] private GameObject editPanel;

        private void Start()
        {
            input.OnCancelPanelEvent += CloseEditPanel;
        }

        public void OpenEditPanel(Vector3 pos)
        {
            CloseEditPanel();
            input.isEdit = true;
            editPanel.transform.position = pos;
            editPanel.SetActive(true);
        }

        private void CloseEditPanel()
        {
            if (!editPanel.activeSelf) return;
            input.isEdit = false;
            editPanel.SetActive(false);
        }
    }
}