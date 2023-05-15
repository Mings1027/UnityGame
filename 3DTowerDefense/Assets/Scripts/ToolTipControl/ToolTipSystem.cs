using System;
using GameControl;
using UnityEngine;

namespace ToolTipControl
{
    public class ToolTipSystem : Singleton<ToolTipSystem>
    {
        private ToolTip toolTip;

        private void Awake()
        {
            toolTip = transform.GetChild(0).GetComponent<ToolTip>();
            toolTip.gameObject.SetActive(false);
        }

        public void Show(Vector3 pos, string content, string header = "")
        {
            toolTip.SetText(pos, content, header);
            toolTip.gameObject.SetActive(true);
        }

        public void Hide()
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}