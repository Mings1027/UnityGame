using UIControl;
using UnityEngine;

namespace TowerControl
{
    public class BaseTower : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            UIManager.Instance.BaseTowerHealth.Damage(1);
        }
    }
}
