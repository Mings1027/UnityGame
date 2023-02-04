using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class IKMover : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform[] legTargets;

    [SerializeField] private float moveSpeed;
    [SerializeField] private AnimationCurve legCurve;
    [SerializeField] private float lerp;

    [SerializeField] private float radius;

    private Vector3 _oldPos, _curPos, _newPos;

    private RaycastHit _hit;

    private void Start()
    {
        lerp = 1;
        _oldPos = _curPos = _newPos = transform.position;
    }

    private void Update()
    {
        Move().Forget();
    }

    private async UniTaskVoid Move()
    {
        for (var i = 0; i < legTargets.Length; i++)
        {
            var r = Physics.Raycast(legTargets[i].position, Vector3.down, out _hit, 10, groundLayer);
            if (r)
            {
                if (Vector3.Distance(_newPos, _hit.point) > radius * 0.5f)
                {
                    lerp = 0;
                    _newPos = _hit.point;
                    _oldPos = legTargets[i].position;
                }
            }

            await MoveLeg(i);
        }
    }

    private async UniTask MoveLeg(int i)
    {
        while (lerp < 1)
        {
            _curPos = Vector3.Lerp(_oldPos, _newPos, lerp);
            _curPos.y = legCurve.Evaluate(lerp);
            lerp += Time.deltaTime * moveSpeed;
            legTargets[i].position = _curPos;
            await UniTask.Yield();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            Gizmos.DrawSphere(_newPos, 0.1f);
        }
    }
}