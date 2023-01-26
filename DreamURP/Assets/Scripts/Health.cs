using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int curHealth, maxHealth;

    [SerializeField] private UnityEvent<GameObject> onHitWithReference, onDeathWithReference;
    [SerializeField] private bool isDead;
    [SerializeField] private float disappearTime;

    public void InitializeHealth(int healthValue)
    {
        curHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
    }

    public void GetHit(int amount, GameObject sender)
    {
        if (isDead) return;
        curHealth -= amount;
        if (curHealth > 0)
        {
            onHitWithReference?.Invoke(sender);
        }
        else
        {
            onDeathWithReference?.Invoke(sender);
            isDead = true;
            this.Wait(disappearTime, () => gameObject.SetActive(false));
        }
    }
}