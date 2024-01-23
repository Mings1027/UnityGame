using CustomEnumControl;
using DataControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace MapControl
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
            PoolObjectManager.Get(PoolObjectKey.ObstacleSmoke, transform.position);
            EarnCoin();

            Destroy(gameObject);
        }

        private void EarnCoin()
        {
            var num = (ushort)(Random.Range(10, 30) * 10);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, transform.position).SetGoldText(num);
            SoundManager.PlayUISound(num < 100 ? SoundEnum.LowCost :
                num < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);
            UIManager.instance.towerGold += num;
        }
    }
}