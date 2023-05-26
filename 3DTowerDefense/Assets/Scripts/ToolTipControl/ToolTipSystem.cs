using UnityEngine;

namespace ToolTipControl
{
    public class ToolTipSystem : MonoBehaviour
    {
        private ToolTip _toolTip;

        private void Awake()
        {
            _toolTip = transform.GetComponentInChildren<ToolTip>();
        }

        public void Show(Transform pos, string content, string header = "")
        {
            _toolTip.SetText(pos, content, header);
        }

        public void Hide()
        {
            _toolTip.gameObject.SetActive(false);
        }
    }
}