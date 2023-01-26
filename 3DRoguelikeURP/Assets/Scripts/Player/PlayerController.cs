using DG.Tweening;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cam;

        [Header("Movement")] [SerializeField] private float moveSpeed;
        [SerializeField] private float turnSmoothTime = 0.25f;

        [Header("Dash")] [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float disToWall, dashPower, isDashingTime;
        [SerializeField] private Ease dashEase;

        private bool isMove, isDash;
        private Vector3 moveVec;
        private Rigidbody rigid;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        public void Move(Vector3 moveVector, bool isAttack)
        {
            if (isDash) return;
            moveVec = moveVector;
            isMove = moveVec.magnitude > 0.1f;
            if (!isMove) return;
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;

            var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            rigid.MovePosition(rigid.position + moveVec * (moveSpeed * Time.fixedDeltaTime));
            if (isAttack) return;
            rigid.MoveRotation(Quaternion.Lerp(rigid.rotation, Quaternion.Euler(0, angle, 0), turnSmoothTime));
        }

        public void Dash()
        {
            if (Physics.Raycast(transform.position, moveVec, disToWall, wallLayer) || !isMove ||
                isDash) return;
            isDash = true;
            rigid.rotation = Quaternion.LookRotation(moveVec);
            rigid.DOMove(rigid.transform.position + moveVec * dashPower, isDashingTime).SetEase(dashEase)
                .OnComplete(() =>
                {
                    isDash = false;
                    rigid.velocity = Vector3.zero;
                    rigid.angularVelocity = Vector3.zero;
                });
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, moveVec * disToWall);
        }
    }
}