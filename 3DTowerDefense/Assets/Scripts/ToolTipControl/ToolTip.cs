using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ToolTipControl
{
    public class ToolTip : MonoBehaviour
    {
        private RectTransform _target;
        // private RectTransform _rectTransform;
        //
        // [SerializeField] private int characterWrapLimit;
        // [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            // _rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

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
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }

            contentField.text = content;
            SetPosition();
            gameObject.SetActive(true);
        }

        private void SetPosition()
        {
            var screenPos = _target.position;
            var pos = _target.localPosition;

            pos.x = screenPos.x > Screen.width * 0.5f ? pos.x - 200 : pos.x + 200;

            transform.localPosition = pos;
        }
    }
}