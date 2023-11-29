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
            var position = transform.position;
            PoolObjectManager.Get(PoolObjectKey.ObstacleSmoke, position);
            var num = Random.Range(10, 500);
            PoolObjectManager.Get<CoinText>(UIPoolObjectKey.CoinText, position).SetText(num);
            
            SoundManager.Instance.PlaySound(num < 100 ? SoundEnum.LowCost :
                num < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);

            UIManager.Instance.TowerCost += num;
            Destroy(gameObject);
        }
    }
}