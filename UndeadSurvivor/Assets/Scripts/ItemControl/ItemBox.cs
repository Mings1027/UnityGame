using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/BoxItem")]
    public class ItemBox : ItemData
    {
        [SerializeField] private ItemData[] itemData;

        public override void Apply(GameObject target)
        {
            var random = Random.Range(0, itemData.Length);
            itemData[random].Apply(target);
        }
    }
}