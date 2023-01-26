using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int curHealth, maxHealth;

    [SerializeField] private UnityEvent onHitWithReference, onDeathWithReference;
    [SerializeField] private bool isDead;
    [SerializeField] private float disappearTime;

    private void Awake()
    {
        InitializeHealth(maxHealth);
    }

    public void InitializeHealth(int healthValue)
    {
        curHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
    }

    public void OnDamage(int amount)
    {
        if (isDead) return;
        curHealth -= amount;
        if (curHealth > 0)
        {
            onHitWithReference?.Invoke();
        }
        else
        {
            onDeathWithReference?.Invoke();
            isDead = true;
            this.Wait(disappearTime, () => gameObject.SetActive(false));
        }
    }
}