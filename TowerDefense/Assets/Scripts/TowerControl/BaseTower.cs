using UIControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace TowerControl
{
    public class BaseTower : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out MonsterUnit monsterUnit);
            UIManager.Instance.BaseTowerHealth.Damage(monsterUnit is BossUnit ? 5 : 1);
            other.gameObject.SetActive(false);
        }
    }
}