using System;
using UnityEngine;

namespace PlayerControl
{
    public class PlayerSpriteController : MonoBehaviour
    {
        private Camera _cam;
        [SerializeField] private Vector3 lastMoveVec;
        [Range(0, 180f)] [SerializeField] private float backAngle = 65f;
        [Range(0, 180f)] [SerializeField] private float sideAngle = 140f;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private PlayerController playerController;
        private static readonly int MoveY = Animator.StringToHash("moveY");
        private static readonly int MoveX = Animator.StringToHash("moveX");
        private static readonly int IsMove = Animator.StringToHash("isMove");
        private static readonly int IdleX = Animator.StringToHash("idleX");
        private static readonly int IdleY = Animator.StringToHash("idleY");

        private bool isMove;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _cam = Camera.main;
            animator = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            playerController = GetComponentInParent<PlayerController>();
            lastMoveVec = transform.forward;
        }

        private void LateUpdate()
        {
            isMove = playerController.moveVec.sqrMagnitude > 0;
            animator.SetBool(IsMove, isMove);
            if (isMove)
            {
                RotateMoveAnimation();
                lastMoveVec = playerController.changedMoveVec;
            }
            else
            {
                RotateIdleAnimation();
            }
        }

        private void RotateMoveAnimation()
        {
            Vector2 animDir;
            var playerMoveVec = playerController.moveVec;

            if (playerMoveVec.z > 0)
            {
                animDir = new Vector2(0, 1);
            }
            else if (playerMoveVec.z < 0)
            {
                animDir = new Vector2(0, -1);
            }
            else if (playerMoveVec.x > 0)
            {
                animDir = new Vector2(1, 0);
                sprite.flipX = false;
            }
            else
            {
                animDir = new Vector2(1, 0);
                sprite.flipX = true;
            }

            animator.SetFloat(MoveX, animDir.x);
            animator.SetFloat(MoveY, animDir.y);
        }

        private void RotateIdleAnimation()
        {
            Vector2 animDir = default;
            var camTransform = _cam.transform;
            var camTransformForward = camTransform.forward;
            var camForwardVec = new Vector3(camTransformForward.x, 0, camTransformForward.z);

            var signedAngle = Vector3.SignedAngle(lastMoveVec, camForwardVec, Vector3.up);
            var angle = Mathf.Abs(signedAngle);

            if (lastMoveVec.z > 0)
            {
                animDir = CheckCameraRotation(angle, signedAngle, animDir);
            }
            else if (lastMoveVec.z < 0)
            {
                animDir = CheckCameraRotation(angle, signedAngle, animDir);
            }
            else if (lastMoveVec.x > 0)
            {
                animDir = CheckCameraRotation(angle, signedAngle, animDir);
                sprite.flipX = false;
            }
            else
            {
                animDir = CheckCameraRotation(angle, signedAngle, animDir);
                sprite.flipX = true;
            }

            animator.SetFloat(IdleX, animDir.x);
            animator.SetFloat(IdleY, animDir.y);
        }

        private Vector2 CheckCameraRotation(float angle, float signedAngle, Vector2 dir)
        {
            if (angle < backAngle)
            {
                dir = new Vector2(0, 1);
            }
            else if (angle < sideAngle)
            {
                dir = new Vector2(1, 0);
                sprite.flipX = signedAngle > 0;
            }
            else
            {
                dir = new Vector2(0, -1);
            }

            return dir;
        }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawRay(mainTransform.position, lastMoveVec * 5);
        // }
    }
}