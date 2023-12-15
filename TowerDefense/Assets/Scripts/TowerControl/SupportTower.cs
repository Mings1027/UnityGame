using System;
using CustomEnumControl;
using EPOOutline;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class SupportTower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TowerType TowerType => towerType;
        public event Action<SupportTower> OnClickTower;
        public Outlinable Outline { get; private set; }

        [SerializeField] private TowerType towerType;

        private void Awake()
        {
            Outline = GetComponent<Outlinable>();
        }

        private void OnEnable()
        {
            UIManager.Instance.Mana.ManaRegenValue++;
            Outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;

            OnClickTower?.Invoke(this);
            Outline.enabled = true;
        }

        public virtual void ObjectDisable()
        {
            UIManager.Instance.Mana.ManaRegenValue--;
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }
    }
}