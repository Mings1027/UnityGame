using GameControl;
using UnityEngine;

namespace ToolTipControl
{
    public class ToolTipSystem : Singleton<ToolTipSystem>
    {
        [SerializeField] private ToolTip toolTip;

        public void Show(string content, string header ="")
        {
            toolTip.SetText(content,header);
            toolTip.gameObject.SetActive(true);
        }

        public void Hide()
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}