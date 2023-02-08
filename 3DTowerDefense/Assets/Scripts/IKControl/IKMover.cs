using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class IKMover : MonoBehaviour
{
    private RaycastHit _hit;
    private Vector3 _prevPos, _curPos, _nextPos;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform[] legs;
    [SerializeField] private Transform[] sensors;
    [SerializeField] private float lerp;
    [SerializeField] private float stepDistance;
    [SerializeField] private float moveSpeed;

    private void Awake()
    {
        lerp = 1;
    }

    private async UniTaskVoid Update()
    {
        for (var i = 0; i < sensors.Length; i++)
        {
            if (IsGrounded())
            {
                if (Vector3.Distance(sensors[i].position, legs[i].position) > stepDistance)
                {
                    _prevPos = legs[i].position;
                    _nextPos = _hit.point;
                    await UniTask.WaitUntil(() => lerp >= 1);
                    Step(i).Forget();
                }
            }
        }
    }

    private async UniTaskVoid Step(int index)
    {
        lerp = 0;
        while (lerp < 1)
        {
            _curPos = Vector3.Lerp(_prevPos, _nextPos, lerp);
            lerp += Time.deltaTime * moveSpeed;
            legs[index].position = _curPos;
            await UniTask.Yield();
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, out _hit, groundLayer);
    }
}