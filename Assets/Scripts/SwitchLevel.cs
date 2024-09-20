using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchLevel : MonoBehaviour
{
    //This variable is used to check if the player is in the gate area
    private bool isPlayerInGateArea = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
  
    void Update()
    {
        //When the player presses the "F" key and he collide with gate area, the game will load the first level

        if (isPlayerInGateArea && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene("LimboHub");

        }



    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInGateArea = true;
        }
    }

    // Detect when the player exits the gate area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInGateArea = false;
        }
    }
    }
