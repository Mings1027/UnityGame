using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private MeshRenderer[] meshs;
    private CharacterController characterController;
    private AnimationController animationController;

    [SerializeField] private Transform cam;
    //Move
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float acceleration = 0.1f;
    private Vector3 moveVec;
    //Dash
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    //Gravity
    // private const float Gravity = -9.8f;
    //Animation
    private float moveValue = 0f;

    [SerializeField] private bool isMove, isDash, isDamage;

    [SerializeField] private int curHealth, maxHealth;

    private void Awake()
    {
        curHealth = maxHealth;
        meshs = GetComponentsInChildren<MeshRenderer>();
        characterController = GetComponent<CharacterController>();
        animationController = GetComponent<AnimationController>();
    }
    public void Move(Vector3 movement)
    {
        if (isDash) return;
        moveVec = movement;
        isMove = moveVec.magnitude > 0.1f;
        if (isMove)
        {
            var targetAngle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), turnSmoothTime);
            moveVec = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        }
        // moveVec.y += Gravity;  you have use use this when you use characterController.Move() method
        characterController.SimpleMove(moveVec * (moveSpeed));
        Animation();
    }
    private void Animation()
    {
        if (isMove && moveValue < 1.0f)
        {
            moveValue += Time.deltaTime * acceleration;
        }
        if (!isMove && moveValue > 0.0f)
        {
            moveValue += Time.deltaTime * -acceleration;
        }
        if (!isMove && moveValue < 0.0f) moveValue = 0.0f;
        // animationController.MovementAnim(moveValue);
    }
    public void Dash()
    {
        if (!isMove) return;
        StartCoroutine(DashCoroutine());
    }
    private IEnumerator DashCoroutine()
    {
        isDash = true;
        transform.rotation = Quaternion.LookRotation(moveVec);
        var startTime = dashTime;
        while (startTime > 0)
        {
            characterController.SimpleMove(moveVec * (dashSpeed));
            startTime -= Time.deltaTime;
            yield return null;
        }
        isDash = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EnemyAttack")) return;
        if (isDamage) return;
        var enemyAttack = other.GetComponent<EnemyAttack>();
        curHealth -= enemyAttack.damage;
        StartCoroutine(OnDamage());
    }

    private IEnumerator OnDamage()
    {
        var wait = new WaitForSeconds(1f);
        isDamage = true;
        foreach (var mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        yield return wait;

        isDamage = false;
        foreach (var mesh in meshs)
        {

            mesh.material.color = Color.white;
        }

    }
}
