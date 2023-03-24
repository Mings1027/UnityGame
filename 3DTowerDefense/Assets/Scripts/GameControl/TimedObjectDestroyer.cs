using DG.Tweening;
using UnityEngine;

namespace GameControl
{
    public class TimedObjectDestroyer : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        
        private void OnEnable()
        {
            // Invoke(nameof(DestroyObject),lifeTime);
            DOVirtual.DelayedCall(lifeTime, () => gameObject.SetActive(false));
        }

        // private void DestroyObject() => gameObject.SetActive(false);
    }
}
