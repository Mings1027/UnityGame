using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class SupportTower : Tower
    {
        public event Action<SupportTower> OnClickTower;

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnClickTower?.Invoke(this);
        }

        public virtual void ObjectDisable()
        {
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }
    }
}