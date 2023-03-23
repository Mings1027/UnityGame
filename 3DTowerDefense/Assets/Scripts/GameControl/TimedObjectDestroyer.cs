using DG.Tweening;
using UnityEngine;

namespace GameControl
{
    public class TimedObjectDestroyer : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        
        private void OnEnable()
        {
            DOVirtual.DelayedCall(lifeTime, () => gameObject.SetActive(false));
        }
    }
}
