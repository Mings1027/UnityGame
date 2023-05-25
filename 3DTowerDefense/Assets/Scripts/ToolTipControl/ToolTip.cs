using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ToolTipControl
{
    public class ToolTip : MonoBehaviour
    {
        private Vector3 _target;
        private RectTransform rectTransform;
        private LayoutElement layoutElement;

        [SerializeField] private int characterWrapLimit;
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            layoutElement = GetComponent<LayoutElement>();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                SetPosition();
            }
        }

        public void SetText(Vector3 pos, string content, string header = "")
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
            var x = _target.x / Screen.width;
            var y = _target.y / Screen.height;

            rectTransform.pivot = new Vector2(x, y);
            transform.position = _target;
        }
    }
}