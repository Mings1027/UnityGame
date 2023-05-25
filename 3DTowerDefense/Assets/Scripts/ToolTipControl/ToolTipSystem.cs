using UnityEngine;

namespace ToolTipControl
{
    public class ToolTipSystem : MonoBehaviour
    {
        private ToolTip toolTip;

        private void Awake()
        {
            toolTip = transform.GetComponentInChildren<ToolTip>();
        }

        public void Show(Vector3 pos, string content, string header = "")
        {
            toolTip.SetText(pos, content, header);
        }

        public void Hide()
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}