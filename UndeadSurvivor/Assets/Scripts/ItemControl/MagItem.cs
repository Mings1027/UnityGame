using PlayerControl;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/MagItem")]
    public class MagItem : ItemEffect
    {
        [SerializeField] private int magTime;

        public override void Apply(GameObject target)
        {
            target.GetComponent<PlayerStatus>().TurnOnMag(magTime).Forget();
        }
    }
}