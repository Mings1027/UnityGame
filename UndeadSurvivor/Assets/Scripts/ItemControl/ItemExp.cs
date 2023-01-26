using PlayerControl;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/ExpItem")]
    public class ItemExp : ItemData
    {
        [SerializeField] private int amount;

        public override void Apply(GameObject target)
        {
            target.GetComponent<PlayerStatus>().CurExp += amount;
        }
    }
}