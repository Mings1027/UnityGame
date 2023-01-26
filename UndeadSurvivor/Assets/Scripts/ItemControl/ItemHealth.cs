using GameControl;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/HealthItem")]
    public class ItemHealth : ItemData
    {
        [SerializeField] private int amount;

        public override void Apply(GameObject target)
        {
            target.GetComponent<Health>().CurHealth += amount;
        }
    }
}