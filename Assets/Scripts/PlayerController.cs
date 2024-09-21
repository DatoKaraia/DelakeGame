using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

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
        if (Input.GetButtonDown("Light Attack"))
        {
            state = 4;
            StartCoroutine(attack(1, weapon));
        }

        else if (Input.GetButtonDown("Heavy Attack"))
        {
            state = 4;
            StartCoroutine(attack(2, weapon));
        }
    }

    IEnumerator attack(int type, PlayerWeapon attackWeapon)
    {
        chainPos = 0;
        int aniLength;
        AnimationClip aniClip;
        PlayerAttack currentAttack = attackWeapon.lightAttacks[0];

        // Light Attack
        if (type == 1)
        {
            currentAttack = attackWeapon.lightAttacks[chainPos];

            animator.Play(currentAttack.attackAniName);
        }
        //Heavy Attack
        else if (type == 2)
        {
            currentAttack = attackWeapon.heavyAttacks[chainPos];

            animator.Play(currentAttack.attackAniName);
        }

        // Wait for animator to Update
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(currentAttack.attackAniName))
        {
            yield return null;
        }

        // Set aniClip to the attack animation
        aniClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        // Set animation Length in frames
        aniLength = currentAttack.cancelableFrames.Length;


        // ----------------------- Run Attack ----------------------- //

        //Go through each animator frame
        for (int i = 0; i < aniLength; i++)
        {
            // Check if colliders should be activated or deactivated
            for (int j = 0; j < currentAttack.attackColliders.Length; j++)
            {
                if (i == currentAttack.attackColliders[j].startFrame)
                {
                    currentAttack.attackColliders[j].col.enabled = true;
                }
                if (i == currentAttack.attackColliders[j].endFrame)
                {
                    currentAttack.attackColliders[j].col.enabled = false;
                }
            }

            Debug.Log(currentAttack.cancelableFrames[i]);

            // Continue Attacking
            if (currentAttack.cancelableFrames[i])
            {
                // Start a new Light Attack
                if (type == 1 && Input.GetButtonDown("Light Attack") && attackWeapon.lightAttacks.Length > chainPos)
                {
                    chainPos++;
                    currentAttack = attackWeapon.lightAttacks[chainPos];

                    animator.Play(currentAttack.attackAniName);

                    yield return null;

                    aniClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                    aniLength = currentAttack.cancelableFrames.Length;
                }
                // Start a new Heavy Attack
                else if (type == 2 && Input.GetButtonDown("Heavy Attack") && attackWeapon.heavyAttacks.Length > chainPos)
                {
                    chainPos++;
                    currentAttack = attackWeapon.heavyAttacks[chainPos];

                    animator.Play(currentAttack.attackAniName);

                    yield return null;

                    aniClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                    aniLength = currentAttack.cancelableFrames.Length;
                }
            }

            // Wait until next frame
            yield return new WaitForSeconds(1 / aniClip.frameRate);
        }

        //Go back to Idle
        chainPos = 0;
        state = 1;
    }
}
