using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace MapControl
{
    public class InteractiveObject : ObstacleObject, IPointerDownHandler, IPointerUpHandler
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
            var gold = (ushort)(Random.Range(10, 30) * 10);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.GoldText, transform.position).SetGoldText(gold);
            SoundManager.PlayUISound(gold < 100 ? SoundEnum.LowCost :
                gold < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);
            GameHUD.IncreaseTowerGold(gold);
        }
    }
}