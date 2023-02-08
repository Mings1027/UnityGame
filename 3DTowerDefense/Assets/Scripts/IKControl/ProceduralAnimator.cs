using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace IKControl
{
    public class ProceduralAnimator : MonoBehaviour
    {
        private Vector3[] _legPositions;
        private Vector3[] _legOriginPositions;
        private Vector3 _velocity;
        private Vector3 _lastBodyPosition;
        private Vector3 _lastVelocity;

        private readonly List<int> _nextIndexToMove = new();
        private readonly List<int> _indexMoving = new();
        private readonly List<int> _oppositeLegIndex = new();

        private bool _currentLeg;

        [SerializeField] private Transform body;
        [SerializeField] private Transform[] legTargets;
        [SerializeField] private Transform[] legCubes;
        [SerializeField] private float moveDistance;

        [SerializeField] private int legMoveSmoothness = 5;
        [SerializeField] private int velocitySmoothness = 3;
        [SerializeField] private float overStepMultiplier = 1.3f;
        [SerializeField] private float bodyJitterCutOff = 0.1f;
        [SerializeField] private float stepHeight = 0.5f;

        private void Awake()
        {
            _lastBodyPosition = body.position;
            _legPositions = new Vector3[legTargets.Length];
            _legOriginPositions = new Vector3[legTargets.Length];
            // legMoveSmoothness = legTargets.Length;
            for (var i = 0; i < legTargets.Length; i++)
            {
                _legPositions[i] = legTargets[i].position;
                _legOriginPositions[i] = legTargets[i].position;

                if (_currentLeg)
                {
                    _oppositeLegIndex.Add(i + 1);
                    _currentLeg = false;
                }
                else
                {
                    _oppositeLegIndex.Add(i - 1);
                    _currentLeg = true;
                }
            }
        }

        private void FixedUpdate()
        {
            var position = body.position;
            _velocity = position - _lastBodyPosition;
            _velocity += velocitySmoothness * _lastVelocity;
            _velocity /= (velocitySmoothness + 1);

            MoveLegs();

            _lastBodyPosition = position;
            _lastVelocity = _velocity;
        }

        private void MoveLegs()
        {
            for (var i = 0; i < legTargets.Length; i++)
            {
                if (Vector3.Distance(legTargets[i].position, legCubes[i].position) >= moveDistance)
                {
                    if (!_nextIndexToMove.Contains(i) && !_indexMoving.Contains(i))
                    {
                        _nextIndexToMove.Add(i);
                    }
                }
                else if (!_nextIndexToMove.Contains(i))
                {
                    legTargets[i].position = _legOriginPositions[i];
                }
            }

            if (_nextIndexToMove.Count == 0 || _indexMoving.Count != 0) return;
            var targetPos = legCubes[_nextIndexToMove[0]].position;
            targetPos += Mathf.Clamp(_velocity.magnitude * overStepMultiplier, 0, 2) *
                         (legCubes[_nextIndexToMove[0]].position - legTargets[_nextIndexToMove[0]].position) +
                         _velocity * overStepMultiplier;
            Step(_nextIndexToMove[0], targetPos, false).Forget();
        }

        private async UniTaskVoid Step(int index, Vector3 moveTo, bool isOpposite)
        {
            if (!isOpposite) MoveOppositeLeg(_oppositeLegIndex[index]);
            if (_nextIndexToMove.Contains(index)) _nextIndexToMove.Remove(index);
            if (!_indexMoving.Contains(index)) _indexMoving.Add(index);

            var startPos = _legOriginPositions[index];
            for (var i = 0; i < legMoveSmoothness; i++)
            {
                var a = i / legMoveSmoothness;
                legTargets[i].position = Vector3.Lerp(startPos,
                    moveTo + new Vector3(0,
                        Mathf.Sign(i / legMoveSmoothness * bodyJitterCutOff) * Mathf.PI * stepHeight, 0), a);
                await UniTask.WaitForFixedUpdate();
            }

            _legOriginPositions[index] = moveTo;

            if (_indexMoving.Contains(index)) _indexMoving.Remove(index);
        }

        private void MoveOppositeLeg(int index)
        {
            var targetPos = legCubes[index].position;
            targetPos += Mathf.Clamp(_velocity.magnitude * overStepMultiplier, 0, 2) *
                         (legCubes[index].position - legTargets[index].position) +
                         _velocity * overStepMultiplier;
            Step(index, targetPos, false).Forget();
        }
    }
}