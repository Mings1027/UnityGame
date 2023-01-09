using GameControl;
using UnityEngine;
using UnityEngine.Events;

namespace EnemyControl
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int curHealth, maxHealth;
        [SerializeField] private UnityEvent<GameObject> onHitWithReference, onDeathWithReference;
        private bool _isLive;


        public void InitializeHealth(SpawnData data)
        {
            _isLive = true;
            maxHealth = data.health;
            curHealth = maxHealth;
        }

        public void GetHit(int amount, GameObject sender)
        {
            if (!_isLive) return;
            curHealth -= amount;
            if (curHealth > 0)
            {
                onHitWithReference?.Invoke(sender);
            }
            else
            {
                onDeathWithReference?.Invoke(sender);
                _isLive = false;
                gameObject.SetActive(false);
            }
        }
    }
}