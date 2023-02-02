using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToolTipControl
{
    public class TowerToolTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ToolTip toolTip;

        public Sprite towerSprite;
        public string towerName;
        public string towerDescription;
        public int towerAtk;
        public int towerDef;

        public void OnPointerEnter(PointerEventData eventData)
        {
            toolTip.ShowToolTip(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            toolTip.HideToolTip();
        }

        private void OnDisable()
        {
            toolTip.HideToolTip();
        }
    }
}