using PlayerControl;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/ExpItem")]
    public class ExpItem : ItemEffect
    {
        [SerializeField] private int exp;

        public override void Apply(GameObject target)
        {
            target.GetComponent<PlayerStatus>().CurExp += exp;
        }
    }
}