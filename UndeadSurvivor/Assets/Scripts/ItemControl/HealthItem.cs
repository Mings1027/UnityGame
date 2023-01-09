using PlayerControl;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/HealthItem")]
    public class HealthItem : ItemEffect
    {
        [SerializeField] private int amount;

        public override void Apply(GameObject target)
        {
            target.GetComponent<PlayerStatus>().CurHealth += amount;
        }
    }
}