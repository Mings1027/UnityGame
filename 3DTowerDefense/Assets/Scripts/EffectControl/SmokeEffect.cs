using GameControl;
using UnityEngine;

namespace EffectControl
{
    public class SmokeEffect : MonoBehaviour
    {
        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}
