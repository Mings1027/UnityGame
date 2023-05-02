using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameControl
{
    [System.Serializable]
    public class Node
    {
        public Node(bool _isWall, int _x, int _z)
        {
            isWall = _isWall;
            x = _x;
            z = _z;
        }

        public bool isWall;
        public Node parentNode;

        // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
        public int x;
        public int z;
        public int g;
        public int h;

        public int F => g + h;
    }


    public class PathFinding : MonoBehaviour
    {
        public Vector3Int bottomLeft, topRight, startPos, targetPos;
        public List<Node> finalNodeList;
        public bool allowDiagonal, dontCrossCorner;
        public float radius;
        public Collider[] wallColliders;

        private int _sizeX, _sizeY;
        private Node[,] _nodeArray;
        private Node _startNode, _targetNode, _curNode;
        private List<Node> _openList, _closedList;

        // startPos가 float 일때
        // public Transform startTR;

        private void Start()
        {
            wallColliders = new Collider[50];
            ASter();
        }

        private void ASter()
        {
            // startPos가 float 일때
            // startPos = Vector2Int.RoundToInt(startTR.position);

            // NodeArray의 크기 정해주고, isWall, x, y 대입
            _sizeX = topRight.x - bottomLeft.x + 1;
            _sizeY = topRight.z - bottomLeft.z + 1;
            _nodeArray = new Node[_sizeX, _sizeY];

            for (var i = 0; i < _sizeX; i++)
            {
                for (var j = 0; j < _sizeY; j++)
                {
                    const bool isWall = false;

                    _nodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.z);

                    // foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y),
                    //              0.4f))
                    //     if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
                    //         isWall = true;
                    //
                    // NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.z);
                }
            }


            // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
            _startNode = _nodeArray[startPos.x - bottomLeft.x, startPos.z - bottomLeft.z];
            _targetNode = _nodeArray[targetPos.x - bottomLeft.x, targetPos.z - bottomLeft.z];

            _openList = new List<Node> { _startNode };
            _closedList = new List<Node>();
            finalNodeList = new List<Node>();


            while (_openList.Count > 0)
            {
                // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
                print(_openList.Count);
                _curNode = _openList[0];
                for (int i = 1; i < _openList.Count; i++)
                    if (_openList[i].F <= _curNode.F && _openList[i].h < _curNode.h)
                        _curNode = _openList[i];

                _openList.Remove(_curNode);
                _closedList.Add(_curNode);


                // 마지막
                if (_curNode == _targetNode)
                {
                    var targetCurNode = _targetNode;
                    while (targetCurNode != _startNode)
                    {
                        finalNodeList.Add(targetCurNode);
                        targetCurNode = targetCurNode.parentNode;
                    }

                    finalNodeList.Add(_startNode);
                    finalNodeList.Reverse();

                    for (var i = 0; i < finalNodeList.Count; i++)
                        print(i + "번째는 " + finalNodeList[i].x + ", " + finalNodeList[i].z);
                    return;
                }


                // ↗↖↙↘
                if (allowDiagonal)
                {
                    OpenListAdd(_curNode.x + 1, _curNode.z + 1);
                    OpenListAdd(_curNode.x - 1, _curNode.z + 1);
                    OpenListAdd(_curNode.x - 1, _curNode.z - 1);
                    OpenListAdd(_curNode.x + 1, _curNode.z - 1);
                }

                // ↑ → ↓ ←
                OpenListAdd(_curNode.x, _curNode.z + 1);
                OpenListAdd(_curNode.x + 1, _curNode.z);
                OpenListAdd(_curNode.x, _curNode.z - 1);
                OpenListAdd(_curNode.x - 1, _curNode.z);
            }
        }

        private void OpenListAdd(int checkX, int checkY)
        {
            // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
            if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.z &&
                checkY < topRight.z + 1 &&
                !_nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.z].isWall &&
                !_closedList.Contains(_nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.z]))
            {
                // 대각선 허용시, 벽 사이로 통과 안됨
                if (allowDiagonal)
                    if (_nodeArray[_curNode.x - bottomLeft.x, checkY - bottomLeft.z].isWall &&
                        _nodeArray[checkX - bottomLeft.x, _curNode.z - bottomLeft.z].isWall)
                        return;

                // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
                if (dontCrossCorner)
                    if (_nodeArray[_curNode.x - bottomLeft.x, checkY - bottomLeft.z].isWall ||
                        _nodeArray[checkX - bottomLeft.x, _curNode.z - bottomLeft.z].isWall)
                        return;


                // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
                var neighborNode = _nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.z];
                var moveCost = _curNode.g + (_curNode.x - checkX == 0 || _curNode.z - checkY == 0 ? 10 : 14);


                // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
                if (moveCost >= neighborNode.g && _openList.Contains(neighborNode)) return;
                
                neighborNode.g = moveCost;
                neighborNode.h = (Mathf.Abs(neighborNode.x - _targetNode.x) +
                                  Mathf.Abs(neighborNode.z - _targetNode.z)) *
                                 10;
                neighborNode.parentNode = _curNode;

                _openList.Add(neighborNode);
            }
        }

        private void OnDrawGizmos()
        {
            if (finalNodeList.Count == 0) return;
            for (var i = 0; i < finalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector3(finalNodeList[i].x, 1, finalNodeList[i].z),
                    new Vector3(finalNodeList[i + 1].x, 1, finalNodeList[i + 1].z));
        }
    }
}