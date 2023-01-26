using UnityEngine;
using UnityEngine.Events;

namespace GameControl
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int curHealth;

        public int CurHealth
        {
            get => curHealth;
            set
            {
                curHealth = value;
                if (curHealth >= maxHealth) curHealth = maxHealth;
            }
        }
    
        [SerializeField] private UnityEvent<GameObject> onHitWithReference, onDeathWithReference;
        private bool _isLive;

        public void InitializeHealth(SpawnData data)
        {
            _isLive = true;
            maxHealth = data.health;
            curHealth = maxHealth;
        }
        private void OnEnable()
        {
            _isLive = true;
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
