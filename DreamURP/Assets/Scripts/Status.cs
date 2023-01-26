using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
    [SerializeField] private UnityEvent<GameObject> onHit, onDeath;
    [SerializeField] private Slider currentHealthImage, currentStaminaImage;

    [SerializeField] private float recoverySpeed;
    [SerializeField] private float maxHealth;
    private float curHealth;

    [SerializeField] private float maxStamina;
    private float curStamina;
    [SerializeField] private float rollStamina;
    [SerializeField] private float attackStamina;
    private bool canAttack, canRoll, isDead;

    [SerializeField] private float disappearTime;

    private void OnEnable()
    {
        curHealth = maxHealth;
        curStamina = maxStamina;
    }

    private void Update()
    {
        currentHealthImage.value = curHealth / maxHealth;
        currentStaminaImage.value = curStamina / maxStamina;
        if (curStamina < maxStamina)
            curStamina += recoverySpeed * Time.deltaTime;
        canRoll = curStamina > rollStamina;
        canAttack = curStamina > attackStamina;
    }

    public void GetHit(float amount, GameObject sender)
    {
        if (isDead) return;
        curHealth -= amount;
        if (curHealth > 0) onHit?.Invoke(sender);
        else
        {
            onDeath?.Invoke(sender);
            isDead = true;
            this.Wait(disappearTime, () => gameObject.SetActive(false));
        }
    }

    public bool RollStamina()
    {
        if (!canRoll) return false;
        curStamina -= rollStamina;
        return true;
    }

    public bool AttackStamina()
    {
        if (!canAttack) return false;
        curStamina -= attackStamina;
        return true;
    }
}