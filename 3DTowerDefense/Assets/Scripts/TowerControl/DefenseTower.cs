using GameControl;

namespace TowerControl
{
    public class DefenseTower : Tower
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