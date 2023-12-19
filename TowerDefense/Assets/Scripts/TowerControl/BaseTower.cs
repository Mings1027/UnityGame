using MonsterControl;
using UIControl;
using UnityEngine;

namespace TowerControl
{
    public class BaseTower : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out MonsterStatus monsterStatus);
            var damage = monsterStatus.BastTowerDamage;
            UIManager.Instance.BaseTowerHealth.Damage(damage);
        }
    }
}