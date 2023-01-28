using GameControl;
using UnityEngine;

namespace TowerControl
{
    public class MeleeTower : Tower
    {
        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        }


        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}