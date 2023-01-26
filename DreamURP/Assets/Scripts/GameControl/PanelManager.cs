using System.Collections.Generic;
using UnityEngine;

namespace GameControl
{
    public static class PanelManager
    {
        public static void PanelControl( Stack<GameObject> panelStack,GameObject openPanel)
        {
            openPanel.SetActive(true);
            panelStack.Peek().SetActive(false);
            panelStack.Push(openPanel);
        }
    }
}