using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rigid;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lerp;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        
    }

    private void MoveToNextWayPoint()
    {
        lerp += Time.deltaTime * moveSpeed;
        _animationCurve.Evaluate(lerp);
    }
}
