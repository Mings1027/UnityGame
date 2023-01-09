using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/BoxItem")]
    public class BoxItem : ItemEffect
    {
        [SerializeField] private ItemEffect[] itemEffects;

        public override void Apply(GameObject target)
        {
            var ranItem = Random.Range(0, itemEffects.Length);
            itemEffects[ranItem].Apply(target);
        }
    }
}