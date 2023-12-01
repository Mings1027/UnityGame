using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class InteractiveObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount != 1) return;
            if (Input.GetTouch(0).deltaPosition != Vector2.zero) return;
            var position = transform.position;
            PoolObjectManager.Get(PoolObjectKey.ObstacleSmoke, position);
            var num = (ushort)(Random.Range(10, 50) * 10);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetText(num);

            SoundManager.Instance.PlaySound(num < 100 ? SoundEnum.LowCost :
                num < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);

            UIManager.Instance.TowerCost += num;
            Destroy(gameObject);
        }
    }
}