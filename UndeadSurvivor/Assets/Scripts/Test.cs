using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Collider2D[] enemies = new Collider2D[10];
    [SerializeField] private float range;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform nearestEnemy;
    [SerializeField] private Camera cam;

    public Rigidbody2D rigid;

    private void Start()
    {
        cam = Camera.main;
    }

    // private async UniTaskVoid DoWait()
    // {
    //     await transform.DOMove(new Vector3(0, 3, 0), 3);
    // }
    

    private void FindNearestEnemy()
    {
        enemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        Transform nearTarget = null;

        if (enemies.Length > 0)
        {
            var nearestDistance = float.MaxValue;
            foreach (var enemy in enemies)
            {
                var distance = Vector2.SqrMagnitude(transform.position - enemy.transform.position);
                if (nearestDistance > distance)
                {
                    nearestDistance = distance;
                    nearTarget = enemy.transform;
                }
            }
        }

        nearestEnemy = nearTarget;
    }
}