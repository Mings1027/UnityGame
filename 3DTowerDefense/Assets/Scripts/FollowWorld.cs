using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    private Camera _cam;

    public Transform target;
    [SerializeField] private Vector3 offset;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        var pos = _cam.WorldToScreenPoint(target.position + offset);

        if (transform.position != pos)
        {
            transform.position = pos;
        }
    }
}