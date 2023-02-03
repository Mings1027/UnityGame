using UnityEngine;
using UnityEngine.EventSystems;

namespace ToolTipControl
{
    public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string header;
        [SerializeField] private string content;

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipSystem.Instance.Show(content, header);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipSystem.Instance.Hide();
        }
    }
}