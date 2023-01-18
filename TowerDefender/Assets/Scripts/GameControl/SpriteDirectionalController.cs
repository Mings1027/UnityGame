using UnityEngine;

namespace GameControl
{
    public class SpriteDirectionalController : MonoBehaviour
    {
        private Camera _cam;
        [Range(0, 180f)] [SerializeField] private float backAngle = 65f;
        [Range(0, 180f)] [SerializeField] private float sideAngle = 155f;
        [SerializeField] private Transform mainTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer sprite;
        private static readonly int IdleY = Animator.StringToHash("idleY");
        private static readonly int IdleX = Animator.StringToHash("idleX");

        private void Awake()
        {
            _cam = Camera.main;
            mainTransform = transform.parent.transform;
            animator = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            RotateAnimation();
        }

        private void RotateAnimation()
        {
            var camTransform = _cam.transform;
            var camTransformForward = camTransform.forward;
            var camForwardVec = new Vector3(camTransformForward.x, 0, camTransformForward.z);
            Debug.DrawRay(camTransform.position, camForwardVec * 5, Color.magenta);

            var signedAngle = Vector3.SignedAngle(mainTransform.forward, camForwardVec, Vector3.up);
            Vector2 animDirection;
            // Debug.DrawRay(mainTransform.position,mainTransform.forward,Color.green);

            var angle = Mathf.Abs(signedAngle);

            if (angle < backAngle)
            {
                // Back animation
                animDirection = new Vector2(0, -1f);
            }
            else if (angle < sideAngle)
            {
                // Side animation
                // Left is flipX true, Right is flipX false
                animDirection = new Vector2(1, 0);
                sprite.flipX = signedAngle < 0;

                // animDirection 
                //     = signedAngle < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
            }
            else
            {
                // Front animation
                animDirection = new Vector2(0, 1);
            }

            animator.SetFloat(IdleX, animDirection.x);
            animator.SetFloat(IdleY, animDirection.y);
        }
    }
}