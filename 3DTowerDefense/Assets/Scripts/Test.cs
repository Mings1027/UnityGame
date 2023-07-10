using DG.Tweening;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Tween _wayPointTween;

    [SerializeField] private Transform wayPointParent;
    [SerializeField] private float speed;
    [SerializeField] private PathType pathType;
    [SerializeField] private PathMode pathMode;
    [SerializeField] private Vector3[] wayPoints;

    private void Awake()
    {
        wayPoints = new Vector3[wayPointParent.childCount];
        for (int i = 0; i < wayPoints.Length; i++)
        {
            wayPoints[i] = wayPointParent.GetChild(i).position;
        }

        _wayPointTween = transform.DOPath(wayPoints, speed, pathType, pathMode).SetAutoKill(false).Pause();
    }

    private void Start()
    {
        _wayPointTween.Restart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _wayPointTween.Play();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            _wayPointTween.Pause();
        }
    }
}