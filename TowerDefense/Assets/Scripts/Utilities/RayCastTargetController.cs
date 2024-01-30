#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class RayCastTargetController : MonoBehaviour
    {
        [ContextMenu("On RayCast Target")]
        private void OnRayCastTarget()
        {
            var images = GetComponentsInChildren<Image>();
            var tmpText = GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < images.Length; i++)
            {
                images[i].raycastTarget = true;
            }

            for (int i = 0; i < tmpText.Length; i++)
            {
                tmpText[i].raycastTarget = true;
            }
        }

        [ContextMenu("Off RayCast Target")]
        private void OffRayCastTarget()
        {
            var images = GetComponentsInChildren<Image>();
            var tmpText = GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < images.Length; i++)
            {
                images[i].raycastTarget = false;
            }

            for (int i = 0; i < tmpText.Length; i++)
            {
                tmpText[i].raycastTarget = false;
            }
        }
    }
}
#endif