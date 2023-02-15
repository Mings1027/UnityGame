using GameControl;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
        // private void OnEnable()
        // {
        //     InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        // }


        // private void OnDisable()
        // {
        //     StackObjectPool.ReturnToPool(gameObject);
        // }
    }
}