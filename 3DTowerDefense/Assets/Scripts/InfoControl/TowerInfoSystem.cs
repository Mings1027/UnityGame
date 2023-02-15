using System;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace InfoControl
{
    public class TowerInfoSystem : Singleton<TowerInfoSystem>
    {
        [SerializeField] private InputManager input;

        [SerializeField] private TowerInfo towerInfo;

        private void Awake()
        {
            input.OnCancelPanelEvent += Hide;
        }

        public void Show(Vector3 pos,string content, string header = "")
        {
            towerInfo.SetText(content, header);
            towerInfo.transform.position = pos;
            towerInfo.gameObject.SetActive(true);
        }

        private void Hide()
        {
            towerInfo.gameObject.SetActive(false);
        }
    }
}