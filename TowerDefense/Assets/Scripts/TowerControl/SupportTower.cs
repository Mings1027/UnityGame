using System;
using CustomEnumControl;
using GameControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class SupportTower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TowerType TowerType => towerType;
        public event Action<SupportTower> OnClickTower;
        [SerializeField] private TowerType towerType;

        private void OnEnable()
        {
            UIManager.Instance.Mana.ManaRegenValue++;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;

            OnClickTower?.Invoke(this);
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