using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange;
    [SerializeField] private LayerMask enemyLayer;

    public void Attack()
    {
        var hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            var value = Random.Range(1, 6);
            enemy.GetComponent<Health>().OnDamage(value);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
