using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth;
    [NonSerialized] public float health;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0)
        {
            health = 0;
        }

        if (health == 0)
        {
        }
        else
        {
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.tag == "Weapon")
        {
            GameObject weapon = other.transform.parent.gameObject;
            health -= weapon.GetComponent<PlayerWeapon>().currentDamage;
        }
    }
}
