using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        if (transform.position != target.position)
            transform.position = target.position;
    }
}
