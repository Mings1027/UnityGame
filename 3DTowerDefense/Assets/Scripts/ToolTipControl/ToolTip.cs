using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ToolTipControl
{
    public class ToolTip : MonoBehaviour
    {
        private float _canvasHalfWidth;
        private RectTransform _rectTransform;

        [SerializeField] private Image towerImage;
        [SerializeField] private TextMeshProUGUI towerName, towerDescription;
        [SerializeField] private TextMeshProUGUI towerAtk, towerDef;

        private void Awake()
        {
            _canvasHalfWidth = GetComponentInParent<CanvasScaler>().referenceResolution.x * 0.5f;
            _rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            transform.position = Mouse.current.position.ReadValue();
            _rectTransform.pivot = _rectTransform.anchoredPosition.x + _rectTransform.sizeDelta.x > _canvasHalfWidth
                ? new Vector2(1, 1)
                : new Vector2(0, 1);
        }

        public void ShowToolTip(TowerToolTipController toolTipController)
        {
            gameObject.SetActive(true);
            towerImage.sprite = toolTipController.towerSprite;
            towerName.text = toolTipController.towerName;
            towerDescription.text = toolTipController.towerDescription;
            towerAtk.text = toolTipController.towerAtk.ToString();
            towerDef.text = toolTipController.towerDef.ToString();
        }

        public void HideToolTip()
        {
            gameObject.SetActive(false);
        }
    }
}