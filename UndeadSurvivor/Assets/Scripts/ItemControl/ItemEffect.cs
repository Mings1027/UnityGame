using UnityEngine;

namespace ItemControl
{
    public abstract class ItemEffect : ScriptableObject
    {
        public abstract void Apply(GameObject target);
    }
}
