using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ToolTipControl
{
    public class ToolTip : MonoBehaviour
    {
        private Transform _target;
        private RectTransform _rectTransform;

        [SerializeField] private int characterWrapLimit;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
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
            _target = pos;

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
            var headerLength = headerField.text.Length;
            var contentLength = contentField.text.Length;

            layoutElement.enabled = headerLength > characterWrapLimit || contentLength > characterWrapLimit;

            var pos = _target.position;
            var pivotX = pos.x / Screen.width;
            var pivotY = pos.y / Screen.height;

            _rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = pos;
        }
    }
}