using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
// using TextControl;
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

            PoolObjectManager.Get(PoolObjectKey.ObstacleSmoke, transform.position);
            var ran = Random.Range(0, 10);
            if (ran < 2)
            {
                HealBaseTower();
            }
            else
            {
                EarnCoin();
            }

            Destroy(gameObject);
        }

        private void HealBaseTower()
        {
            UIManager.Instance.BaseTowerHealth.Heal(1);
        }

        private void EarnCoin()
        {
            var num = (ushort)(Random.Range(10, 30) * 10);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, transform.position).SetCostText(num);

            SoundManager.Instance.PlaySound(num < 100 ? SoundEnum.LowCost :
                num < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);

            UIManager.Instance.TowerCost += num;
        }
    }
}