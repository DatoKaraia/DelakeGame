using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Movement Variables
    private Rigidbody rb;
    private Collider col;
    private Animator animator;

    [SerializeField] GameObject cameraController;

    public float jumpForce, distToGround = .5f;
    public float gravity = 4;

    [NonSerialized] public float xMomentum, yMomentum, yAngleFinal, yAngleMovement, yAngleCamera, speed = 1;
    int state = 1;
    bool isGrounded;
    RaycastHit groundRaycast;
    Vector3 deltaVelocity;

    //Attack Variables
    [SerializeField] PlayerWeapon weapon;
    [SerializeField] GameObject leftHandPivot;
    int chainPos = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        xMomentum = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));

        yAngleCamera = cameraController.transform.rotation.eulerAngles.y;
        yAngleMovement = (Mathf.Rad2Deg * Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        transform.rotation = Quaternion.Euler(0f, yAngleFinal, 0f);

        /* Uses a State Machine like code to switch between walking, jumping, idle etc.
         * 
         * States:
         * 0:Falling, 1:Idle, 2:Walking, 3:Running, 4:Attacking
        */

        if (!isGrounded)
        {
            state = 0;
        }

        //Falling
        if (state == 0)
        {
            if (isGrounded)
            {
                state = 1;
                animator.ResetTrigger("Jump");
            }
        }

        //Idle
        if (state == 1)
        {
            if (xMomentum >= .2f)
            {
                state = 2;
            }

            checkJump();
            checkAttack();
        }

        //Walking
        if (state == 2)
        {
            if (Input.GetButton("Run"))
            {
                state = 3;
            }

            else if (xMomentum < .2f)
            {
                state = 1;
            }


            yAngleFinal = yAngleMovement + yAngleCamera;
            checkJump();
            checkAttack();
        }

        //Running
        if (state == 3)
        {
            if (!Input.GetButton("Run"))
            {
                state = 2;
            }

            yAngleFinal = yAngleMovement + yAngleCamera;
            checkJump();
            checkAttack();
        }


        animator.SetInteger("State", state);
    }

    void FixedUpdate()
    {
        isGrounded = Physics.SphereCast(col.bounds.center, .25f, Vector3.down, out groundRaycast, distToGround, LayerMask.GetMask("Default"));

        if (state == 0)
        {
            yMomentum -= gravity * Time.deltaTime;
        }
        else {
            yMomentum = 0;
        }

        deltaVelocity = new Vector3(0, yMomentum, 0);
        rb.velocity += deltaVelocity;
    }

    void checkJump()
    {
        if (Input.GetButton("Jump"))
        {
            state = 0;
            animator.SetTrigger("Jump");
            yMomentum = jumpForce;
        }
    }

    void checkAttack()
    {
        if (Input.GetButton("Light Attack"))
        {
            StartCoroutine(attack(1, weapon));
        }

        else if (Input.GetButton("Heavy Attack"))
        {
            StartCoroutine(attack(2, weapon));
        }
    }

    IEnumerator attack(int type, PlayerWeapon attackWeapon)
    {
        if (state != 4)
        {
            state = 4;

            //Light attack
            if (type == 1)
            {
                animator.Play(Animator.StringToHash(attackWeapon.lightAttacks[chainPos].attackAniName));
            }
            else if (type == 2) 
            {
                animator.Play(Animator.StringToHash(attackWeapon.heavyAttacks[chainPos].attackAniName));
            }
        }

        yield return null;
    }
}
