using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private int curHealth, maxHealth;

    private NavMeshAgent agent;
    private Rigidbody rigid;
    private KnockBack knockBack;
    public Transform player;
    public float distance;
    [SerializeField] private bool isChase;

    public float range;
    public Transform centrePoint; //if you want enemy move everywhere then centrePoint is child this object
    //but you want fixed range then centrePoint separate this object

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        knockBack = GetComponent<KnockBack>();
    }

    private void Update()
    {
        isChase = Vector3.Distance(transform.position, player.position) < distance ? true : false;
        if (isChase) ChasePlayer();
        else Patrol();

    }
    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }
    private void Patrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                // Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                agent.SetDestination(point);
            }
        }
    }
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    private void GetHit()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerAttack")) return;
        var playerAttack = other.GetComponent<PlayerAttack>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(centrePoint.position, range);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distance);
    }

}
