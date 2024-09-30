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
    private PlayerInputController playerInput;

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

    //Dodge Variables
    [SerializeField] int iFrameStart, iFrameEnd;
    [SerializeField] AnimationClip dodgeAni;
    int dodgeAniLength = 28;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInputController>();
    }

    void Update()
    {
        //xMomentum = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));
        xMomentum = playerInput.move.ReadValue<Vector2>().magnitude;

        yAngleCamera = cameraController.transform.rotation.eulerAngles.y;
        //yAngleMovement = (Mathf.Rad2Deg * Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        yAngleMovement = Mathf.Rad2Deg * Mathf.Atan2(playerInput.move.ReadValue<Vector2>().x, playerInput.move.ReadValue<Vector2>().y);
        transform.rotation = Quaternion.Euler(0f, yAngleFinal, 0f);

        /* Uses a State Machine like code to switch between walking, jumping, idle etc.
         * 
         * States:
         * 0:Falling, 1:Idle, 2:Walking, 3:Running, 4:Attacking, 5: Dodging
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
            checkDodge();
        }

        //Walking
        if (state == 2)
        {
            if (playerInput.sprint.raw.IsPressed())
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
            checkDodge();
        }

        //Running
        if (state == 3)
        {
            if (!playerInput.sprint.raw.IsPressed())
            {
                state = 2;
            }

            yAngleFinal = yAngleMovement + yAngleCamera;
            checkJump();
            checkAttack();
            checkDodge();
        }

        if (state == 5)
        {

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
        if (playerInput.jump.raw.IsPressed())
        {
            state = 0;
            animator.SetTrigger("Jump");
            yMomentum = jumpForce;
        }
    }

    void checkAttack()
    {
        if (playerInput.lightAttack.inBufferDown())
        {
            state = 4;
            StartCoroutine(attack(1, weapon));
        }

        else if (playerInput.heavyAttack.inBufferDown())
        {
            state = 4;
            StartCoroutine(attack(2, weapon));
        }
    }

    void checkDodge()
    {
        if (playerInput.dodge.inBufferDown())
        {
            state = 5;
            /*
            if (cameraController.GetComponent<CameraController>().targetLockOn != null)
            {
                animator.SetFloat("X Normalised", Input.GetAxis("Vertical"));
                animator.SetFloat("Y Normalised", Input.GetAxis("Horizontal"));
            }
            else
            {
                animator.SetFloat("X Normalised", 1);
                animator.SetFloat("Y Normalised", 0);
            }
            */

            StartCoroutine(dodge());
        }
    }

    IEnumerator dodge()
    {
        animator.Play("5: Dodge");

        for (int i = 0; i < dodgeAniLength; i++)
        {
            // Check if colliders should be activated or deactivated
            

            // Continue Dodging or Cancel Dodge
            if (i >= 20)
            {
                // Dodge Again
                if (playerInput.dodge.inBufferDown(5))
                {

                    animator.Play("5: Dodge");

                    while (!animator.GetCurrentAnimatorStateInfo(0).IsName("5: Dodge"))
                    {
                        yield return null;
                    }

                    i = 0;
                }

                // Attack
                else if (playerInput.lightAttack.inBufferDown(5))
                {
                    state = 4;
                    StartCoroutine(attack(1, weapon));
                    yield break;
                }

                else if (playerInput.heavyAttack.inBufferDown(5))
                {
                    state = 4;
                    StartCoroutine(attack(2, weapon));
                    yield break;
                }

                // Walk
                else if (xMomentum >= .2f)
                {
                    state = 2;
                    animator.Play("2: Walking");
                    yield break;
                }
            }

            // Wait until next frame
            yield return new WaitForSeconds(1 / dodgeAni.frameRate);
        }

        //Go back to Idle
        state = 1;
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
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(currentAttack.attackAniName))
        {
            yield return null;
        }

        // Set aniClip to the attack animation
        aniClip = currentAttack.attackClip;
        // Set animation Length in frames
        aniLength = currentAttack.cancelableFrames.Length;

        // ----------------------- Do Attack ----------------------- //

        //Go through each animator frame
        for (int i = 0; i < aniLength; i++)
        {
            // Check if colliders should be activated or deactivated
            for (int j = 0; j < currentAttack.attackColliders.Length; j++)
            {
                if (i == currentAttack.attackColliders[j].startFrame)
                {
                    currentAttack.attackColliders[j].col.enabled = true;
                    attackWeapon.currentDamage = attackWeapon.weaponDamage * currentAttack.attackColliders[j].motionValue;
                }
                if (i == currentAttack.attackColliders[j].endFrame)
                {
                    currentAttack.attackColliders[j].col.enabled = false;
                }
            }

            // Continue Attacking or Cancel Attack
            if (currentAttack.cancelableFrames[i])
            {
                if (playerInput.dodge.inBufferDown(5))
                {
                    state = 5;
                    StartCoroutine(dodge());
                    yield break;
                }

                else if (playerInput.heavyAttack.inBufferDown(20) && attackWeapon.heavyAttacks.Length > chainPos + 1 && type == 2)
                {
                    chainPos++;
                    for (int j = 0; j < currentAttack.attackColliders.Length; j++)
                    {
                        currentAttack.attackColliders[j].col.enabled = false;
                    }

                    currentAttack = attackWeapon.heavyAttacks[chainPos];
                    aniClip = currentAttack.attackClip;
                    aniLength = currentAttack.cancelableFrames.Length;

                    animator.Play(currentAttack.attackAniName);

                    while (!animator.GetCurrentAnimatorStateInfo(0).IsName(currentAttack.attackAniName))
                    {
                        yield return null;
                    }

                    i = 0;
                }

                else if (playerInput.lightAttack.inBufferDown(20) && attackWeapon.lightAttacks.Length > chainPos + 1 && type == 1)
                {
                    chainPos++;
                    for (int j = 0; j < currentAttack.attackColliders.Length; j++)
                    {
                        currentAttack.attackColliders[j].col.enabled = false;
                    }

                    currentAttack = attackWeapon.lightAttacks[chainPos];
                    aniClip = currentAttack.attackClip;
                    aniLength = currentAttack.cancelableFrames.Length;

                    animator.Play(currentAttack.attackAniName);

                    while (!animator.GetCurrentAnimatorStateInfo(0).IsName(currentAttack.attackAniName))
                    {
                        yield return null;
                    }

                    i = 0;
                }

                else if (xMomentum >= .2f)
                {
                    state = 2;

                    yield break;
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
