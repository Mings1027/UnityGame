using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToolTipControl
{
    public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ToolTipSystem toolTipSystem;
        [SerializeField] private string header;
        [SerializeField] [Multiline] private string content;

        private void Awake()
        {
            toolTipSystem = ToolTipSystem.Instance;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            toolTipSystem.Show(transform.position, content, header);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            toolTipSystem.Hide();
        }
    }
}