using UnityEngine;

namespace ItemControl
{
    public abstract class ItemData : ScriptableObject
    {
        public abstract void Apply(GameObject target);
    }
}