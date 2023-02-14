using System;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ToolTipControl
{
    [ExecuteInEditMode]
    public class ToolTip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        [SerializeField] private LayoutElement layoutElement;

        [SerializeField] private int wrapLimit;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetText(string content, string header = "")
        {
            var pos = Mouse.current.position.ReadValue();
            var x = pos.x / Screen.width;
            var y = pos.y / Screen.height;
            
            _rectTransform.pivot = new Vector2(x, y);
            transform.position = pos;

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

            var headerLength = headerField.text.Length;
            var contentLength = contentField.text.Length;

            layoutElement.enabled = headerLength > wrapLimit || contentLength > wrapLimit;
        }
    }
}