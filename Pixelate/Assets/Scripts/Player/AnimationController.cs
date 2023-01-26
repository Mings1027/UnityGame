using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator anim;

    private static readonly int IsMove = Animator.StringToHash("isMove");
    private static readonly int Movement = Animator.StringToHash("movement");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void MoveAnimation(bool isMove)
    {
        anim.SetBool(IsMove, isMove);
    }
    public void MovementAnim(float moveValue)
    {
        anim.SetFloat(Movement, moveValue);
    }
}
