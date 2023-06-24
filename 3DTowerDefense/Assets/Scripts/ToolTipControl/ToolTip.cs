using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ToolTipControl
{
    public class ToolTip : MonoBehaviour
    {
        private RectTransform _target;

        [SerializeField] private TextMeshProUGUI headerText;

        [SerializeField] private TextMeshProUGUI contentText;

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                SetPosition();
            }
        }

        public void SetText(Transform pos, string content, string header = "")
        {
            _target = pos.GetComponent<RectTransform>();

            if (string.IsNullOrEmpty(header))
            {
                headerText.gameObject.SetActive(false);
            }
            else
            {
                headerText.gameObject.SetActive(true);
                headerText.text = header;
            }

            contentText.text = content;
            SetPosition();
        }

        private void SetPosition()
        {
            var screenPos = _target.position;
            var pos = _target.localPosition;

            pos.x = screenPos.x > Screen.width * 0.5f ? pos.x - 700 : pos.x + 700;

            transform.localPosition = pos;
        }
    }
}