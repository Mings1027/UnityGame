using PlayerAttack;
using UnityEngine;

namespace ItemControl
{
    [CreateAssetMenu(menuName = "Item/BulletItem")]
    public class ItemBullet : ItemData
    {
        public override void Apply(GameObject target)
        {
            var t = target.transform.GetChild(2).gameObject;
            if (!t.activeSelf) t.SetActive(true);

            var orbit = t.GetComponent<OrbitAttack>();
            orbit.AddBullet();
        }
    }
}