using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ToolTipControl
{
    [ExecuteInEditMode]
    public class ToolTip : MonoBehaviour
    {
        private Vector2 _position;

        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        public void SetText(Vector3 pos, string content, string header = "")
        {
            var screenWidth = Screen.width;
            var x = pos.x > screenWidth * 0.5f ? pos.x - 600 : pos.x + 600;

            transform.position = new Vector3(x, transform.position.y);

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
        }
    }
}