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
            var ran = Random.Range(0, 10);

            if (ran < 2)
            {
                if (UIManager.Instance.GetTowerHealth().IsFull)
                {
                    EarnCoin();
                }
                else
                {
                    HealBaseTower();
                }
            }
            else
            {
                EarnCoin();
            }

            Destroy(gameObject);
        }

        private void HealBaseTower()
        {
            var towerHp = UIManager.Instance.GetTowerHealth();
            towerHp.CurInteractiveTransform(transform);
            towerHp.Heal(1);
        }

        private void EarnCoin()
        {
            var num = (ushort)(Random.Range(10, 30) * 10);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, transform.position).SetGoldText(num);
            SoundManager.Instance.PlayUISound(num < 100 ? SoundEnum.LowGold :
                num < 250 ? SoundEnum.MediumGold : SoundEnum.HighGold);
            UIManager.Instance.towerGold += num;
        }
    }
}