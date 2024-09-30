using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    // Here if you need it, probably use the buffer system when it's done
    [NonSerialized] public InputAction look, move;
    [NonSerialized] public InputBuffer sprint, jump, lockOn, lightAttack, heavyAttack, dodge;
    int fixedFrameCount = -1;

    // Start is called before the first frame update
    void Start()
    {
        look = InputSystem.actions.FindAction("Look");
        move = InputSystem.actions.FindAction("Move");

        sprint = new InputBuffer(InputSystem.actions.FindAction("Sprint"));
        lockOn = new InputBuffer(InputSystem.actions.FindAction("Lock On"));
        dodge = new InputBuffer(InputSystem.actions.FindAction("Dodge"));
        jump = new InputBuffer(InputSystem.actions.FindAction("Jump"));
        lightAttack = new InputBuffer(InputSystem.actions.FindAction("Light Attack"));
        heavyAttack = new InputBuffer(InputSystem.actions.FindAction("Heavy Attack"));
    }

    void FixedUpdate()
    {
        fixedFrameCount++;

        sprint.frameCount = fixedFrameCount;
        lockOn.frameCount = fixedFrameCount;
        dodge.frameCount = fixedFrameCount;
        jump.frameCount = fixedFrameCount;
        lightAttack.frameCount = fixedFrameCount;
        heavyAttack.frameCount = fixedFrameCount;
    }

    public class InputBuffer
    {
        public InputAction raw;
        int framePressed = -1, frameReleased = -1;
        public int frameCount;
        
        public InputBuffer(InputAction input)
        {
            this.raw = input;
            this.raw.started += context => { this.framePressed = this.frameCount; Debug.Log("Done"); };
            this.raw.performed += context => { this.frameReleased = this.frameCount; };
        }

        public bool inBufferDown(int deltaFrames = 1, InputBuffer[] excludeInput = null)
        {
            if (this.framePressed >= this.frameCount - deltaFrames)
            {
                if (excludeInput != null)
                {
                    foreach (InputBuffer inputs in excludeInput)
                    {
                        if (this.framePressed >= inputs.framePressed)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                
                return true;
            }

            return false;
        }
    }
}
