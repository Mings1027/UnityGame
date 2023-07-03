using UnityEngine;

namespace ToolTipControl
{
    public class ToolTipSystem : MonoBehaviour
    {
       [SerializeField] private ToolTip toolTip;

        private void Awake()
        {
            toolTip.gameObject.SetActive(false);
        }

        public void Show(Transform pos, string content, string header = "")
        {
            toolTip.gameObject.SetActive(true);
            toolTip.SetText(pos, content, header);
        }

        public void Hide()
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}